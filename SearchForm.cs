// Copyright Eric Chauvin 2017 - 2018.
// My blog is at:
// ericsourcecode.blogspot.com

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace CodeEditor
{
  public partial class SearchForm : Form
  {
  private string SearchText = "";


  public SearchForm()
    {
    InitializeComponent();
    }



  private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
    if( e.KeyCode == Keys.Enter ) //  && (e.Alt || e.Control || e.Shift))
      {
      SearchText = SearchTextBox.Text.Trim();
      DialogResult = DialogResult.OK;
      Close();
      }
    }



  internal string GetSearchText()
    {
    return SearchText;
    }



  }
}
