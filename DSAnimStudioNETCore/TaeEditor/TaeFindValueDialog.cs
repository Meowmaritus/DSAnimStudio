using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio.TaeEditor
{
    public partial class TaeFindValueDialog : Form
    {
        public TaeEditorScreen EditorRef;

        private List<TaeFindResult> Results = new List<TaeFindResult>();

        public TaeEditorScreen.FindInfoKeep LastFindInfo = null;

        //private Thread ResultsThread = null;

        public TaeFindValueDialog()
        {
            InitializeComponent();
        }

        private TaeEditorScreen.FindInfoKeep.TaeSearchType CurrentSearchType
        {
            get
            {
                if (comboBoxSearchType.SelectedIndex < 0)
                    comboBoxSearchType.SelectedIndex = 0;
                return (TaeEditorScreen.FindInfoKeep.TaeSearchType)comboBoxSearchType.SelectedIndex;
            }
        }

        private void BuildResults()
        {
            listViewResults.VirtualListSize = Results.Count;
            //listViewResults.Items.Clear();
            //listViewResults.BeginUpdate();
            //listViewResults.SuspendLayout();
            //listViewResults.VirtualMode = false;
            //foreach (var r in Results)
            //{
            //    listViewResults.Items.Add(new ListViewItem(r.ToListViewData()));
            //}
            //listViewResults.EndUpdate();
            //listViewResults.ResumeLayout();

            labelResults.Text = $"Results ({Results.Count}):";
        }

        private void TaeFindValueDialog_Load(object sender, EventArgs e)
        {
            if (LastFindInfo != null)
            {
                textBoxSearchQuery.Text = LastFindInfo.SearchQuery;
                Results = LastFindInfo.Results;
                checkBox1.Checked = LastFindInfo.MatchEntireString;

                BuildResults();

                listViewResults.Items[LastFindInfo.HighlightedIndex].Selected = true;
                listViewResults.Items[LastFindInfo.HighlightedIndex].Focused = true;
                listViewResults.Items[LastFindInfo.HighlightedIndex].EnsureVisible();

                listViewResults.Select();

                listViewResults.Focus();

                comboBoxSearchType.SelectedIndex = (int)LastFindInfo.SearchType;
            }
            else
            {
                comboBoxSearchType.SelectedIndex = 0;
            }
        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void DoSearch()
        {
            //if (ResultsThread != null)
            //    LoadingTaskMan.AbortThread(ResultsThread);
            //ResultsThread?.Abort();

            var st = CurrentSearchType;
            if (st == TaeEditorScreen.FindInfoKeep.TaeSearchType.ParameterValue)
            {
                Results = TaeFindParameter.FindParameterValue(EditorRef, textBoxSearchQuery.Text, checkBox1.Checked);
            }
            else if (st == TaeEditorScreen.FindInfoKeep.TaeSearchType.ParameterName)
            {
                Results = TaeFindParameter.FindParameterName(EditorRef, textBoxSearchQuery.Text, checkBox1.Checked);
            }
            else if (st == TaeEditorScreen.FindInfoKeep.TaeSearchType.EventName)
            {
                Results = TaeFindParameter.FindEventTypeName(EditorRef, textBoxSearchQuery.Text, checkBox1.Checked);
            }
            else if (st == TaeEditorScreen.FindInfoKeep.TaeSearchType.EventType)
            {
                if (int.TryParse(textBoxSearchQuery.Text, out int asInt))
                {
                    Results = TaeFindParameter.FindEventTypeNum(EditorRef, asInt);
                }
                else
                {
                    MessageBox.Show("Search query was not a valid number, which is required " +
                        "for an Event Type Num search. If you wish to search for an event name " +
                        "rather than the type number, change the search type to Event Name.");
                    return;
                }
            }

            
            BuildResults();
        }

        private void ButtonSearch_Click(object sender, EventArgs e)
        {
            DoSearch();
        }

        private void ListViewResults_ItemActivate(object sender, EventArgs e)
        {
            Results[listViewResults.SelectedIndices[0]].GoThere(EditorRef);
            EditorRef.LastFindInfo = new TaeEditorScreen.FindInfoKeep()
            {
                Results = Results,
                HighlightedIndex = listViewResults.SelectedIndices[0],
                SearchQuery = textBoxSearchQuery.Text,
                MatchEntireString = checkBox1.Checked,
                SearchType = (TaeEditorScreen.FindInfoKeep.TaeSearchType)comboBoxSearchType.SelectedIndex,
            };

            //Close();
        }

        private void TextBoxSearchQuery_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                DoSearch();
            }
        }

        private void ListViewResults_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = new ListViewItem(Results[e.ItemIndex].ToListViewData());
        }
    }
}
