using System;
using System.Collections.Generic;
using System.Text;
using Sprocket;
using Sprocket.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Content;
using Sprocket.Web.Controls;

namespace Sprocket.Web.CMS.Admin
{
	public class AdminMenuLink : IRankable, IComparable
	{
		private string name, link;
		private ObjectRank rank;

		public AdminMenuLink(string name, string link, ObjectRank rank)
		{
			this.name = name;
			this.link = link;
			this.rank = rank;
		}

		public ObjectRank Rank
		{
			set { rank = value; }
			get { return rank; }
		}

		public virtual string Render()
		{
			if(link == "" || link == null)
				return name;

			return string.Format("<a class=\"AdminMenuLink\" id=\"AdminMenuLink_{2}\" href=\"{0}\">{1}</a>",
				link, name, link.Trim('/').Replace(':','_').Replace('/','_').Replace('\\','_').Replace('.','_').Replace(' ','_'));
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
				return -1;
			return name.CompareTo(obj.ToString());
		}

		public override string ToString()
		{
			return name;
		}
	}

	public class AdminSection : IRankable
	{
		private string text = null;
		private SprocketScript script = null;
		private Template template = null;
		private ObjectRank rank;

		public AdminSection(string text, ObjectRank rank)
		{
			this.text = text;
			this.rank = rank;
		}

		public AdminSection(Template template, ObjectRank rank)
		{
			this.template = template;
			this.rank = rank;
		}

		public AdminSection(SprocketScript script, ObjectRank rank)
		{
			this.script = script;
			this.rank = rank;
		}

		public AdminSection(object text, ObjectRank rank)
		{
			this.text = text.ToString();
			this.rank = rank;
		}

		public ObjectRank Rank
		{
			set { rank = value; }
			get { return rank; }
		}

		public virtual string Render()
		{
			if (text != null)
				return text;
			if (script != null)
				return script.Execute();
			if (template != null)
				return template.Render();
			return String.Empty;
		}
	}

	public class AdminInterface
	{
		private List<AdminMenuLink> menuLinks = new List<AdminMenuLink>();
		private List<AdminMenuLink> footerLinks = new List<AdminMenuLink>();
		private List<AdminSection> preContentSections = new List<AdminSection>();
		private List<AdminSection> subContentSections = new List<AdminSection>();
		private List<AdminSection> headSections = new List<AdminSection>();
		private List<AdminSection> menuSections = new List<AdminSection>();
		private List<AdminSection> bodyOnLoadScripts = new List<AdminSection>();
		private List<AdminSection> interfaceScripts = new List<AdminSection>();
		private string websiteName = "SprocketCMS";

		internal string WebsiteName
		{
			get { return websiteName; }
			set { websiteName = value; }
		}

		protected Dictionary<WebControlScript, bool> loadedScripts = new Dictionary<WebControlScript, bool>();
		public void AddInterfaceScript(WebControlScript script)
		{
			if (loadedScripts.ContainsKey(script)) return;
			loadedScripts.Add(script, true);
			interfaceScripts.Add(new AdminSection(WebControls.GetScript(script), ObjectRank.First));
		}

		public void AddInterfaceScript(AdminSection script)
		{
			interfaceScripts.Add(script);
		}

		public void AddMainMenuLink(AdminMenuLink link)
		{
			menuLinks.Add(link);
		}

		public void AddFooterLink(AdminMenuLink link)
		{
			footerLinks.Add(link);
		}

		public void AddPreContentSection(AdminSection section)
		{
			preContentSections.Add(section);
		}

		public void AddSubContentSection(AdminSection section)
		{
			subContentSections.Add(section);
		}

		public void AddHeadSection(AdminSection section)
		{
			headSections.Add(section);
		}

		public void AddLeftColumnSection(AdminSection section)
		{
			menuSections.Add(section);
		}

		public void AddBodyOnLoadScript(AdminSection script)
		{
			bodyOnLoadScripts.Add(script);
		}

		public KeyValuePair<string, object>[] GetScriptVariables()
		{
			interfaceScripts.Sort(RankedObject.SortByRank);
			JavaScriptCollection jsc = new JavaScriptCollection();
			int scrnum = 0;
			foreach (AdminSection str in interfaceScripts)
				jsc.Add("sprocket-admin-script-" + scrnum++, str.Render());
			headSections.Add(new AdminSection(jsc.CreateScriptTags(), ObjectRank.Last));

			menuLinks.Sort(RankedObject.SortByRank);
			footerLinks.Sort(RankedObject.SortByRank);
			preContentSections.Sort(RankedObject.SortByRank);
			subContentSections.Sort(RankedObject.SortByRank);
			headSections.Sort(RankedObject.SortByRank);
			bodyOnLoadScripts.Sort(RankedObject.SortByRank);

			StringBuilder menu = new StringBuilder();
			foreach (AdminMenuLink link in menuLinks)
				menu.AppendFormat("<div class=\"main-menu-link\">{0}</div>", link.Render());

			menuSections.Add(new AdminSection(menu.ToString(), ObjectRank.Late));
			menuSections.Sort(RankedObject.SortByRank);

			StringBuilder footer = new StringBuilder();
			foreach (AdminMenuLink link in footerLinks)
			{
				if (footer.Length > 0)
					footer.Append(" | ");
				footer.Append(link.Render());
			}

			StringBuilder preContent = new StringBuilder();
			foreach (AdminSection section in preContentSections)
				preContent.Append(section.Render());

			StringBuilder subContent = new StringBuilder();
			foreach (AdminSection section in subContentSections)
				subContent.Append(section.Render());

			StringBuilder head = new StringBuilder();
			foreach (AdminSection section in headSections)
				head.Append(section.Render());

			StringBuilder left = new StringBuilder();
			foreach (AdminSection section in menuSections)
				left.Append(section.Render());

			StringBuilder onLoad = new StringBuilder();
			foreach (AdminSection script in bodyOnLoadScripts)
			{
				string scr = script.Render();
				if (!scr.Trim().EndsWith(";"))
					scr += ";";
				onLoad.Append(scr);
			}

			KeyValuePair<string, object>[] vars = new KeyValuePair<string, object>[7];
			vars[0] = new KeyValuePair<string, object>("_admin_head", head.ToString());
			vars[1] = new KeyValuePair<string, object>("_admin_mainmenu", left.ToString());
			vars[2] = new KeyValuePair<string, object>("_admin_precontent", preContent.ToString());
			vars[3] = new KeyValuePair<string, object>("_admin_subcontent", subContent.ToString());
			vars[4] = new KeyValuePair<string, object>("_admin_footer", footer.ToString());
			vars[5] = new KeyValuePair<string, object>("_admin_websitename", websiteName);
			vars[6] = new KeyValuePair<string, object>("_admin_bodyonload", onLoad.ToString());
			return vars;
		}
	}
}
