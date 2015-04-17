//Sean Lyons
//12/5/14
//Final Project - WikiPro - Search through wikipedia articles, find random ones, save links with custom names and notes
//utilizes search techniques, serialization, array lists, regular lsts, multithreading
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;

namespace WindowsFormsApplication1
{
    //allows information to be serialized
    [Serializable]
    public partial class Form1 : Form
    {
        //formatter for serialization
        BinaryFormatter formatter = new BinaryFormatter();
        //array lists for saved data to be saved from and loaded into
        ArrayList savedLinkName = new ArrayList();
        ArrayList savedLinks = new ArrayList();
        ArrayList savedNotes = new ArrayList();
        //list so that it can be assigned to linkLabel type
        List<LinkLabel> linkLabels = new List<LinkLabel>();
        //threads to open and save all data simultaneously
        static Thread openThreadOne;
        static Thread openThreadTwo;
        static Thread openThreadThree;
        static Thread saveThreadOne;
        static Thread saveThreadTwo;
        static Thread saveThreadThree;

        public Form1()
        {
            InitializeComponent();
            //initialize and start open/read threads for saved data
            openThreadOne = new Thread(new ThreadStart(OpenThreadOne));
            openThreadTwo = new Thread(new ThreadStart(OpenThreadTwo));
            openThreadThree = new Thread(new ThreadStart(OpenThreadThree));
            openThreadOne.Start();
            openThreadTwo.Start();
            openThreadThree.Start();
            //sleep allows threads to complete fully before link labels are created
            Thread.Sleep(100);
            //calls method responsible for creating/recreating link labels
            LinkLabelCreate();
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //methods responsible for loading saved data. Seperated for multithreading.
        //try catch in case the files don't exist/can't be found
        //open, deserialize, close streams
        // try/catch in case it's first time using program or files cannot be found
        private void OpenThreadOne()
        {
            try
            {
                Stream input = File.OpenRead("savedLinkName.txt");
                savedLinkName = (ArrayList)formatter.Deserialize(input);
                input.Close();
            }
            catch
            {
                return;
            }
        }
        private void OpenThreadTwo()
        {
            try
            {
                Stream input2 = File.OpenRead("savedLinks.txt");
                savedLinks = (ArrayList)formatter.Deserialize(input2);
                input2.Close();
            }
            catch
            {
                return;
            }
        }
        private void OpenThreadThree()
        {
            try
            {
                Stream input3 = File.OpenRead("savedNotes.txt");
                savedNotes = (ArrayList)formatter.Deserialize(input3);
                input3.Close();
            }
            catch
            {
                return;
            }
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void linkClick(object sender, EventArgs e)
        {
            //get name of link from link label being clicked
            string linkName = Convert.ToString(sender.GetType().GetProperty("Text").GetValue(sender, null));
            string linkNameMatch = "";
            int i = 0;
            //find array list position for link so that link notes can be opened
            while (linkNameMatch != Convert.ToString(linkLabels[i].Text))
            {
                //check each saved link name against the link that was clicked to find which link it is
                linkNameMatch = savedLinkName[i].ToString();
                if (linkNameMatch != linkName)
                    i++;
            }
            //set textbox to saved notes
            txtBox.Text = savedNotes[i].ToString();
            //open saved link
            Uri wikiBrowserURL = new Uri(savedLinks[i].ToString());
            wikiBrowser.Navigate(wikiBrowserURL);
        }

        //method to refresh web browser with new wiki article
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Uri wikiBrowserURL =  new Uri("http://en.wikipedia.org/wiki/Special:Random");
            wikiBrowser.Navigate(wikiBrowserURL);
            //remove notes from last page if there were any
            txtBox.Text = "";
        }

        //method for saving page name and link, along with notes
        private void btnSave_Click(object sender, EventArgs e)
        {
            //assign linkName, link, and notes to variables
            string linkName = txtLinkName.Text.ToString();
            string link = wikiBrowser.Url.ToString();
            string notes = txtBox.Text;
            //add to respective strings
            savedLinkName.Add(linkName);
            savedLinks.Add(link);
            savedNotes.Add(notes);

            //calls method responsible for creating/recreating link labels
            LinkLabelCreate();
            
            //initialize and start threads to save data
            saveThreadOne = new Thread(new ThreadStart(SaveThreadOne));
            saveThreadTwo = new Thread(new ThreadStart(SaveThreadTwo));
            saveThreadThree = new Thread(new ThreadStart(SaveThreadThree));
            saveThreadOne.Start();
            saveThreadTwo.Start();
            saveThreadThree.Start();  
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //methods used for saving, seperated for multithreading.
        //create file, save specified array list, close
        public void SaveThreadOne()
        {
            Stream output = File.Create("savedLinkName.txt");
            formatter.Serialize(output, savedLinkName);
            output.Close();
        }
        public void SaveThreadTwo()
        {
            Stream output2 = File.Create("savedLinks.txt");
            formatter.Serialize(output2, savedLinks);
            output2.Close();
        }
        public void SaveThreadThree()
        {
            Stream output3 = File.Create("savedNotes.txt");
            formatter.Serialize(output3, savedNotes);
            output3.Close();
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //method for button to save notes on an already saved link
        private void btnSaveNotes_Click(object sender, EventArgs e)
        {
            string linkName = wikiBrowser.Url.ToString();
            //while loop to get position of link, which will be the same as notes
            int i = 0;
            while (savedLinks[i].ToString() != linkName)
            {
                //increment up until position of link in array list is found
                i++;
            }
            

            //save notes to correct position
            savedNotes[i] = txtBox.Text;
            //delete old array list, recreate with new notes
            File.Delete("savedNotes.txt");
            Stream output = File.Create("savedNotes.txt");
            formatter.Serialize(output, savedNotes);
            output.Close();
        }

        private void btnDeletePage_Click(object sender, EventArgs e)
        {
            //get inputted link to be deleted
            string deleteLink = txtDeleteBox.Text;
            int z = 0;
            while (savedLinkName[z].ToString() != deleteLink)
            {
                z++;
            }
            //remove all associated link information
            savedLinkName.RemoveAt(z);
            savedLinks.RemoveAt(z);
            savedNotes.RemoveAt(z);

            //initialize and start threads to save data
            saveThreadOne = new Thread(new ThreadStart(SaveThreadOne));
            saveThreadTwo = new Thread(new ThreadStart(SaveThreadTwo));
            saveThreadThree = new Thread(new ThreadStart(SaveThreadThree));
            saveThreadOne.Start();
            saveThreadTwo.Start();
            saveThreadThree.Start();
            //restart application, couldn't figure out any other way to remove link labels for deleted links
            Application.Restart();            
        }

        public void LinkLabelCreate()
        {
            //recreate link list with added label
            for (int i = 0; i < savedLinks.Count; i++)
            {
                //set label properties
                linkLabels.Add(
                     new LinkLabel()
                     {
                         Name = "link" + i.ToString()
                     });
                linkLabels[i].Text = savedLinkName[i].ToString();
                linkLabels[i].Click += new EventHandler(linkClick);
                if (i == 0)
                    linkLabels[i].Location = new Point(linkLabel1.Location.X, linkLabel1.Location.Y + 20);
                else
                    linkLabels[i].Location = new Point(linkLabels[i - 1].Location.X, linkLabels[i - 1].Location.Y + 22);
                //add controls to page
                tabPage1.Controls.Add(linkLabels[i]);
            }
        }
    }
}
