﻿using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace martinhromek.library
{
  public partial class Home : System.Web.UI.Page
  {
    #region Properties
    public string DB { set; get; }
    public string DBPath { set; get; } = "~/library/storage.xml";
    public List<Book> Library { private set; get; } = new List<Book>();
    public string LibraryLastUpdate { private set; get; }
    public int NumberOfBooks { get; private set; }
    public int NumberOfReaded { get; private set; }
    public double NumberOfReadedPercentage { get; private set; }
    public int NoOfNew { get; private set; }
    public int NoOfAntic { get; private set; }
    public Book MyOldestBook { get; set; }
    public Book OldestBook { get; set; }
    #endregion

    #region Events
    protected void Page_Load(object sender, EventArgs e)
    {
      try
      {
        LoadData();
        SetStatistics();
        ShowData();
      }
      catch (Exception ex)
      {
        ;
      }
    }

    protected void btnChangeLanguage_Click(object sender, EventArgs e)
    {
      if (Session["lang"] == null || Session["lang"].Equals("cz"))
        Session["lang"] = "en";
      else
        Session["lang"] = "cz";

      Page.Response.Redirect(Request.Url.ToString());
    }
    #endregion

    #region Method
    private void LoadData()
    {
      DB = Server.MapPath(DBPath);

      if (!File.Exists(DB))
        CreateDB();

      Session["db"] = DBPath;

      XmlDocument lDoc = new XmlDocument();
      lDoc.Load(DB);
      XmlNode lRoot = lDoc.DocumentElement;

      LibraryLastUpdate = lRoot.Attributes["last_update"].Value;
      lblLastUpdate.Text = $"last update: {LibraryLastUpdate}";

      foreach (XmlNode lNode in lRoot.ChildNodes)
        if (lNode.Name.Equals("book"))
          Library.Add(new Book(lNode));

      Session["library"] = Library;
    }

    private void CreateDB()
    {
      new XDocument(
        new XElement("books",
          new XAttribute("last_update", DateTime.Now)
        )
      ).Save(DB);
    }

    private void ShowData()
    {
      bool language = (Session["lang"] == null || Session["lang"].Equals("cz")) ? true : false;

      foreach (string group in Enum.GetNames(typeof(Common.BookGroup)))
      {
        this.MainPanel.Controls.Add(
            new Panel() { ID = $"panel{group}", GroupingText = group });
      }

      Library.Sort((x, y) => string.Compare(x.AuthorLastName, y.AuthorLastName));
      foreach (var item in Library)
      {
        ShowBook(FindControl($"panel{item.Groupe}"), item, language);
      }
    }

    private void ShowBook(System.Web.UI.Control panel, Book item, bool lang)
    {
      string tooltip = $"{item.AuthorFirstName} {item.AuthorMiddleName} {item.AuthorLastName}, {item.OriginalName}, {item.CzechName}, {item.OriginalPublicationDate} ({item.MyPublicationDate}), {item.Publish3r}, {item.BookType}, [{item.GenreAll}], [{item.Groupe}], přečteno: {(item.Readed)}, {item.DateRead}, [{item.Ratings} : {item.RatingsValue}%]";
      string bookName = (lang) ? item.CzechName : item.OriginalName;

      panel.Controls.Add(new Label()
      { /*ID = DateTime.Now.Millisecond.ToString(),*/
        Text = $"{item.AuthorLastName}, {item.AuthorFirstName} {item.AuthorMiddleName} - {bookName}",
        Width = 400,
        BorderWidth = 0,
        ToolTip = tooltip,
        ForeColor = item.Readed ? System.Drawing.Color.LawnGreen : System.Drawing.Color.Black
      });
      panel.Controls.Add(new LiteralControl("<br />"));
    }

    private void SetStatistics()
    {
      NoOfBooks();
      NoOfReaded();
      NoOfNewAntic();
      FindOldestBook();

      lblNoOfBooks.Text = $"{NumberOfBooks.ToString()} [n:{NoOfNew}, a:{NoOfAntic}]";
      lblReaded.Text = $"{NumberOfReaded} [{NumberOfReadedPercentage}%]";
      lblMyOldestBook.Text = $"{MyOldestBook.CzechName} [{MyOldestBook.MyPublicationDate}]";
      lblOldestBook.Text = $"{OldestBook.CzechName} [{OldestBook.OriginalPublicationDate}]";
    }

    private void NoOfBooks()
    {
      NumberOfBooks = Library.Count;
    }

    private void NoOfReaded()
    {
      NumberOfReaded = Library.Where(i => i.Readed).Count();
      NumberOfReadedPercentage = Math.Round(Convert.ToDouble(NumberOfReaded) / ((double)NumberOfBooks / 100), 0);
    }

    private void NoOfNewAntic()
    {
      NoOfNew = Library.Where(i => i.BookType.Equals(Common.BookType.Nova.ToString())).Count();
      NoOfAntic = Library.Where(i => i.BookType.Equals(Common.BookType.Antikvariat.ToString())).Count();
    }

    private void FindOldestBook()
    {
      MyOldestBook = Library.OrderBy((x) => (x.MyPublicationDate)).First();
      OldestBook = Library.OrderBy((x) => (x.OriginalPublicationDate)).First();
    }
    #endregion
  }
}