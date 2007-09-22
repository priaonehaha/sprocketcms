using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using Sprocket;
using Sprocket.Security;
using Sprocket.Web;
using Sprocket.Web.Cache;
using Sprocket.Data;
using Sprocket.Utility;

namespace Sprocket.Web.FileManager
{
	[ModuleDependency(typeof(WebEvents))]
	[ModuleDependency(typeof(DatabaseManager))]
	[ModuleDependency(typeof(SecurityProvider))]
	[ModuleDependency(typeof(ContentCache))]
	[ModuleTitle("File Manager")]
	[ModuleDescription("Handles storage and transmission of physical files to and from the client")]
	public class FileManager : ISprocketModule
	{
		public static FileManager Instance
		{
			get { return (FileManager)Core.Instance[typeof(FileManager)].Module; }
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			DatabaseManager.Instance.OnDatabaseHandlerLoaded += new NotificationEventHandler<IDatabaseHandler>(Database_OnDatabaseHandlerLoaded);
		}

		IFileManagerDataLayer dataLayer = null;
		public static IFileManagerDataLayer DataLayer
		{
			get { return FileManager.Instance.dataLayer; }
		}

		void Database_OnDatabaseHandlerLoaded(IDatabaseHandler source)
		{
			source.OnInitialise += new InterruptableEventHandler(DatabaseHandler_OnInitialise);
			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(IFileManagerDataLayer)))
			{
				IFileManagerDataLayer layer = (IFileManagerDataLayer)Activator.CreateInstance(t);
				if (layer.DatabaseHandlerType == source.GetType())
				{
					dataLayer = layer;
					break;
				}
			}
		}

		void DatabaseHandler_OnInitialise(Result result)
		{
			if (!result.Succeeded)
				return;
			if (dataLayer == null)
				result.SetFailed("FileManager has no implementation for " + DatabaseManager.DatabaseEngine.Title);
			else
				dataLayer.InitialiseDatabase(result);
		}

		public void TransmitRequestedImage()
		{
			// note: if processing ever gets this far, we definitely don't have a cached version
			// of this file, or the ContentCache would have served it up by now.

			FileInfo info = new FileInfo(SprocketPath.Physical);
			SizingOptions options = SizingOptions.Parse(info.Name);
			HttpContext.Current.Response.ContentType = "image/jpeg";
			if (WebUtility.RawQueryString == "nocache")
			{
				ResizeImage(options, HttpContext.Current.Response.OutputStream);
				HttpContext.Current.Response.End();
			}
			else
			{
				MemoryStream stream = new MemoryStream();
				ResizeImage(options, stream);
				string path = ContentCache.Store("Sprocket.Web.FileManager.CachedImage." + options.SprocketFileID + "." + options.Filename, null, false, SprocketPath.Value, "image/jpeg", stream);
				HttpContext.Current.Response.TransmitFile(path);
			}
		}

		public void TransmitImage(SizingOptions options)
		{
			MemoryStream stream = new MemoryStream();
			ResizeImage(options, stream);
			HttpContext.Current.Response.ContentType = "image/jpeg";
			HttpContext.Current.Response.BinaryWrite(stream.ToArray());
			HttpContext.Current.Response.End();
		}

		public void DeleteFile(SprocketFile file)
		{
			ContentCache.ClearMultiple("Sprocket.Web.FileManager.CachedImage." + file.SprocketFileID + ".%");
			dataLayer.Delete(file);
		}

		public void ResizeImage(SizingOptions options, Stream outStream)
		{
			Image sourceImage;
			if (options.Image == null)
			{
				SprocketFile file = dataLayer.SelectSprocketFile(options.SprocketFileID, true);
				sourceImage = Image.FromStream(new MemoryStream(file.FileData));
				if (options.DisplayType == SizingOptions.Display.Constrain && options.Width == 0 && options.Height == 0)
				{
					outStream.Write(file.FileData, 0, file.FileData.Length);
					if (outStream.CanSeek)
						outStream.Seek(0, SeekOrigin.Begin);
					return;
				}
			}
			else
				sourceImage = options.Image;

			Size imageSize;
			if (options.DisplayType == SizingOptions.Display.Constrain)
			{
				int maxWidth = 0, maxHeight = 0;
				if (options.Width > 0)
					maxWidth = options.Width - options.Padding * 2 - options.BorderSize * 2;
				if (options.Height > 0)
					maxHeight = options.Height - options.Padding * 2 - options.BorderSize * 2;
				if (maxWidth == 0 && maxHeight == 0)
					throw new Exception("Can't resize image. No dimensions were specified.");
				float srcRatio = (float)sourceImage.Width / (float)sourceImage.Height;
				if (maxWidth == 0)
					imageSize = new Size((int)((float)maxHeight * srcRatio) + options.BorderSize * 2 + options.Padding * 2,
						maxHeight + options.BorderSize * 2 + options.Padding * 2);
				else if (maxHeight == 0)
					imageSize = new Size(maxWidth + options.BorderSize * 2 + options.Padding * 2,
						(int)((float)maxWidth / srcRatio) + options.BorderSize * 2 + options.Padding * 2);
				else
				{
					float destRatio = (float)maxWidth / (float)maxHeight;
					if (destRatio < srcRatio) // landscape; keep width, set height
						imageSize = new Size(maxWidth + options.BorderSize * 2 + options.Padding * 2,
							(int)((float)maxWidth / srcRatio) + options.BorderSize * 2 + options.Padding * 2);
					else // portrait; keep height, set width
						imageSize = new Size((int)((float)maxHeight * srcRatio) + options.BorderSize * 2 + options.Padding * 2,
							maxHeight + options.BorderSize * 2 + options.Padding * 2);
				}
			}
			else
			{
				imageSize = new Size(options.Width, options.Height);
			}
			if (options.PreventEnlargement)
			{
				if (imageSize.Width - options.Padding * 2 - options.BorderSize * 2 > sourceImage.Width)
					imageSize.Width = sourceImage.Width + options.Padding * 2 + options.BorderSize * 2;
				if (imageSize.Height - options.Padding * 2 - options.BorderSize * 2 > sourceImage.Height)
					imageSize.Height = sourceImage.Height + options.Padding * 2 + options.BorderSize * 2;
			}
			using (Image finalImage = new Bitmap(imageSize.Width, imageSize.Height))
			{
				using (Graphics gfx = Graphics.FromImage(finalImage))
				{
					// set the rendering defaults
					gfx.CompositingQuality = CompositingQuality.HighQuality;
					gfx.SmoothingMode = SmoothingMode.HighQuality;
					gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

					// set colours and pens
					Brush fill = new SolidBrush(options.BackgroundColor);
					Pen pen = new Pen(options.BorderColor, options.BorderSize);
					pen.Alignment = PenAlignment.Inset;

					// fill the background
					gfx.FillRectangle(fill, 0, 0, finalImage.Width, finalImage.Height);

					// calculate the area in which we can paint the image
					Rectangle clientRect = new Rectangle(
						options.BorderSize + options.Padding, options.BorderSize + options.Padding,
						finalImage.Width - options.BorderSize * 2 - options.Padding * 2,
						finalImage.Height - options.BorderSize * 2 - options.Padding * 2);

					// work out the letterboxing ratios (if the inner ratio is greater, the photo is
					// proportionally wider than the container, i.e. standard widescreen letterbox)
					float containerRatio = (float)clientRect.Width / (float)clientRect.Height;
					float imgRatio = (float)sourceImage.Width / (float)sourceImage.Height;
					bool landscape = imgRatio > containerRatio;

					Rectangle destRect = new Rectangle(clientRect.X, clientRect.Y, clientRect.Width, clientRect.Height);
					switch (options.DisplayType)
					{
						case SizingOptions.Display.Letterbox:
							if (landscape)
							{
								int h = (int)((float)destRect.Width / imgRatio);
								destRect.Y += (destRect.Height - h) / 2;
								destRect.Height = h;
							}
							else
							{
								int w = (int)((float)destRect.Height * imgRatio);
								destRect.X += (destRect.Width - w) / 2;
								destRect.Width = w;
							}
							gfx.DrawImage(sourceImage, destRect);
							break;

						case SizingOptions.Display.Constrain:
						case SizingOptions.Display.Stretch:
							gfx.DrawImage(sourceImage, destRect);
							break;

						case SizingOptions.Display.Center:
							gfx.DrawImage(sourceImage, destRect, new Rectangle(
								(sourceImage.Width - destRect.Width) / 2, (sourceImage.Height - destRect.Height) / 2,
								destRect.Width, destRect.Height), GraphicsUnit.Pixel);
							break;

						case SizingOptions.Display.Tile:
							int tileX, tileY = clientRect.Y;
							do
							{
								tileX = clientRect.X;
								do
								{
									destRect = new Rectangle(tileX, tileY,
										tileX + sourceImage.Width > clientRect.Right ? clientRect.Right - tileX : sourceImage.Width,
										tileY + sourceImage.Height > clientRect.Bottom ? clientRect.Bottom - tileY : sourceImage.Height);
									gfx.DrawImageUnscaledAndClipped(sourceImage, destRect);
									tileX += sourceImage.Width + options.TileSpacing;
								} while (tileX < clientRect.Right);
								tileY += sourceImage.Height + options.TileSpacing;
							} while (tileY < clientRect.Bottom);
							break;

						case SizingOptions.Display.Crop:
							switch (options.Anchor)
							{
								case SizingOptions.CropAnchor.Top:
									using (Bitmap bmp = new Bitmap(destRect.Width, (int)((float)destRect.Width / imgRatio)))
									{
										using (Graphics gfx2 = Graphics.FromImage(bmp))
										{
											gfx2.InterpolationMode = InterpolationMode.HighQualityBicubic;
											gfx2.DrawImage(sourceImage, 0, 0, bmp.Width, bmp.Height);
										}
										gfx.DrawImage(bmp, destRect, new Rectangle(0, 0, destRect.Width, destRect.Height), GraphicsUnit.Pixel);
									}
									break;

								case SizingOptions.CropAnchor.Bottom:
									using (Bitmap bmp = new Bitmap(destRect.Width, (int)((float)destRect.Width / imgRatio)))
									{
										using (Graphics gfx2 = Graphics.FromImage(bmp))
										{
											gfx2.InterpolationMode = InterpolationMode.HighQualityBicubic;
											gfx2.DrawImage(sourceImage, 0, 0, bmp.Width, bmp.Height);
										}
										gfx.DrawImage(bmp, destRect, new Rectangle(0, bmp.Height - destRect.Height, destRect.Width, destRect.Height), GraphicsUnit.Pixel);
									}
									break;

								case SizingOptions.CropAnchor.Left:
									using (Bitmap bmp = new Bitmap((int)((float)destRect.Height * imgRatio), destRect.Height))
									{
										using (Graphics gfx2 = Graphics.FromImage(bmp))
										{
											gfx2.InterpolationMode = InterpolationMode.HighQualityBicubic;
											gfx2.DrawImage(sourceImage, 0, 0, bmp.Width, bmp.Height);
										}
										gfx.DrawImage(bmp, destRect, new Rectangle(0, 0, destRect.Width, destRect.Height), GraphicsUnit.Pixel);
									}
									break;

								case SizingOptions.CropAnchor.Right:
									using (Bitmap bmp = new Bitmap((int)((float)destRect.Height * imgRatio), destRect.Height))
									{
										using (Graphics gfx2 = Graphics.FromImage(bmp))
										{
											gfx2.InterpolationMode = InterpolationMode.HighQualityBicubic;
											gfx2.DrawImage(sourceImage, 0, 0, bmp.Width, bmp.Height);
										}
										gfx.DrawImage(bmp, destRect, new Rectangle(bmp.Width - destRect.Width, 0, destRect.Width, destRect.Height), GraphicsUnit.Pixel);
									}
									break;

								case SizingOptions.CropAnchor.Center:
									int midW, midH;
									if (landscape)
									{
										midH = destRect.Height;
										midW = (int)((float)destRect.Height * imgRatio);
									}
									else
									{
										midW = destRect.Width;
										midH = (int)((float)destRect.Width / imgRatio);
									}
									using (Bitmap bmp = new Bitmap(midW, midH))
									{
										using (Graphics gfx2 = Graphics.FromImage(bmp))
										{
											gfx2.InterpolationMode = InterpolationMode.HighQualityBicubic;
											gfx2.DrawImage(sourceImage, 0, 0, bmp.Width, bmp.Height);
										}
										if (landscape)
											gfx.DrawImage(bmp, destRect, new Rectangle((bmp.Width - destRect.Width) / 2, 0, destRect.Width, destRect.Height), GraphicsUnit.Pixel);
										else
											gfx.DrawImage(bmp, destRect, new Rectangle(0, (bmp.Height - destRect.Height) / 2, destRect.Width, destRect.Height), GraphicsUnit.Pixel);
									}
									break;

								default:
									break;
							}
							break;

						default:
							break;
					}

					// draw the border last to ensure it doesn't get obscured;
					if (options.BorderSize > 0)
						gfx.DrawRectangle(pen, 0, 0, finalImage.Width - 1, finalImage.Height - 1);

					// find the jpeg encoder
					ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
					ImageCodecInfo encoder = null;
					for (int i = 0; i < encoders.Length; i++)
						if (encoders[i].MimeType == "image/jpeg")
						{
							encoder = encoders[i];
							break;
						}
					if (encoder == null)
						throw new SprocketException("Can't create a thumbnail because no JPEG encoder exists.");
					EncoderParameters prms = new EncoderParameters(1);

					// set the jpeg quality
					prms.Param[0] = new EncoderParameter(Encoder.Quality, options.JpegQuality);

					finalImage.Save(outStream, encoder, prms);
					if(outStream.CanSeek)
						outStream.Seek(0, SeekOrigin.Begin);
				}

			}
			if (options.Image == null)
				sourceImage.Dispose();
		}
	}
}
