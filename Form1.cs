using System;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Threading;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections.Generic;
using netshWrapper;
using System.Collections;
using ListViewSorter;

namespace DDoSMitigator
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
              lvwColumnSorter = new ListViewColumnSorter();
              this.listView1.ListViewItemSorter = lvwColumnSorter;
        }

        private DDoSMitigatorIPsec ipSec;

        private void Form1_Load(object sender, EventArgs e)
        {
            ipSec = new DDoSMitigatorIPsec();
            ipSec.init();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            new Thread(fetchNetstat).Start();
        }

        Dictionary<IPAddress, object[]> connections = new Dictionary<IPAddress, object[]>(); 

        private void fetchNetstat()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            connections.Clear();

            foreach (TcpConnectionInformation info in ipProperties.GetActiveTcpConnections())
            {
                IPAddress address = info.RemoteEndPoint.Address;
                if (address.ToString() == "127.0.0.1")
                    continue;
                int port = info.LocalEndPoint.Port;
                if (connections.ContainsKey(address))
                {
                    int connectionCount = (int)connections[address][0];
                    List<int> ports = (List<int>)connections[address][1];
                    if (!ports.Contains(port))
                        ports.Add(port);
                    connectionCount++;
                    connections[address] = new object[] { connectionCount, ports };
                }
                else
                {
                    List<int> ports =  new List<int>();
                    ports.Add(port);
                    connections.Add(address, new object[] { 1, ports });
                }
            }

            foreach (KeyValuePair<IPAddress, object[]> pair in connections)
            {
                string[] lvis = new string[3];
                lvis[0] = pair.Key.ToString();
                lvis[1] = pair.Value[0].ToString();

                string portsString = "";
                List<int> ports = (List<int>)pair.Value[1];
                foreach (int port in ports)
                    portsString += port + ", ";
                portsString = portsString.Substring(0, portsString.Length - 2);

                lvis[2] = portsString;

                addListViewItemEntry(new ListViewItem(lvis));
            }
        }

        private delegate void addListViewItemEntryDelegate(ListViewItem lvi);
        private void addListViewItemEntry(ListViewItem lvi)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new addListViewItemEntryDelegate(addListViewItemEntry), new object[] { lvi });
            }
            else
            {
                listView1.Items.Add(lvi);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int threshold = int.Parse(thresholdTextBox.Text);
            int blockCount = 0;
            foreach (KeyValuePair<IPAddress, object[]> pair in connections)
            {
                string ip = pair.Key.ToString();
                int connectionCount = (int)pair.Value[0];
                if (connectionCount > threshold)
                {
                    block(ip);
                    blockCount++;
                }
            }
            MessageBox.Show("Blocked " + blockCount + " ips!");
            button6_Click(null, null);
        }

        private void block(string ip)
        {
            /*ProcessStartInfo psi = new ProcessStartInfo("cports");
            psi.WorkingDirectory = Program.path;
            psi.Arguments = "/close * * " + ip + " *";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "runas";
            Process.Start(psi).WaitForExit();*/
            if (ip == "127.0.0.1")
                return;
            IPsec.addFilter(ipSec.DDoSMitigationFilterList, ip);
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        class ListViewItemComparer : IComparer
        {
            private int col;
            private SortOrder order;
            public ListViewItemComparer()
            {
                col = 0;
                order = SortOrder.Ascending;
            }
            public ListViewItemComparer(int column, SortOrder order)
            {
                col = column;
                this.order = order;
            }
            public int Compare(object x, object y) 
            {
                int returnVal= -1;
                returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
                                        ((ListViewItem)y).SubItems[col].Text);
                // Determine whether the sort order is descending.
                if (order == SortOrder.Descending)
                    // Invert the value returned by String.Compare.
                    returnVal *= -1;
                return returnVal;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://whois.domaintools.com/"+ Ip +"/"); 
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button5.Enabled = false;
            checkBox1.Enabled = true;
            textBox1.Enabled = true;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            checkBox5.Enabled = false;
            checkBox6.Enabled = true;
            checkBox7.Enabled = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ipSec.reloadFilterList();
            IPsec.Filter[] filters = IPsec.getFilters(ipSec.DDoSMitigationFilterList);
            listView2.Items.Clear();
            foreach (IPsec.Filter filter in filters)
            {
                if(filter.address.ToString() != "1.1.1.1")
                    listView2.Items.Add(filter.address.ToString());
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            new Thread(fetchNetstat).Start();
        }

        private ListViewColumnSorter lvwColumnSorter;

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView myListView = (ListView)sender;

            // Determine if clicked column is already the column that is being sorted.

            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.

                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.

                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.

            myListView.Sort();
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = checkBox8.Checked;
            button1.Enabled = !checkBox8.Checked;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

    }
}
