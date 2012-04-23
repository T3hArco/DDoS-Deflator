using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;

namespace netshWrapper
{
    public class IPsec
    {
        public struct FilterList
        {
            public string name;
            public int count;
        }

        public struct FilterAction
        {
            public string name;
            public string action;
        }

        public struct FilterPolicy
        {
            public string name;
            public int rules;
            public bool assigned;
        }

        public struct Filter
        {
            public IPAddress address;
        }

        public struct FilterRule
        {
            public bool enabled;
            public FilterList filterList;
            public FilterAction filterAction;
        }

        public static FilterList[] getFilterLists()
        {
            List<FilterList> ret = new List<FilterList>();
            string result = exec("show filterlist all format=table");
            if (result.Contains("IPsec[05067]"))
                return new FilterList[0];
            string[] parts = result.Replace("\r", "").Split('\n');
            for (int i = 2; i < parts.Length - 5; i++)
            {
                string part = parts[i];
                string[] cells = part.Split('\t');
                FilterList filterList = new FilterList();
                filterList.name = cells[0].Trim();
                filterList.count = int.Parse(cells[1].Trim());
                ret.Add(filterList);
            }
            return ret.ToArray();
        }

        public static FilterAction[] getFilterActions()
        {
            List<FilterAction> ret = new List<FilterAction>();
            string result = exec("show filteraction all format=table");
            if (result.Contains("IPsec[05068]"))
                return new FilterAction[0];
            string[] parts = result.Replace("\r", "").Split('\n');
            for (int i = 2; i < parts.Length - 5; i++)
            {
                string part = parts[i];
                string[] cells = part.Split('\t');
                FilterAction filterAction = new FilterAction();
                filterAction.name = cells[0].Trim();
                filterAction.action = cells[1].Trim();
                ret.Add(filterAction);
            }
            return ret.ToArray();
        }

        public static Filter[] getFilters(FilterList list)
        {
            if (list.count == 0)
                return new Filter[0];
            List<Filter> ret = new List<Filter>();
            string result = exec("show filterlist name=\"" + list.name + "\" level=verbose format=table wide=no");
            result = result.Substring(result.IndexOf("-------\r\n") + "-------\r\n".Length);
            string[] parts = result.Replace("\r", "").Split('\n');

            for (int i = 0; i < parts.Length - 2; i += 2)
            {
                string part = parts[i];
                string[] cells = part.Split('\t');
                Filter filter = new Filter();
                filter.address = IPAddress.Parse(cells[1].Trim());
                ret.Add(filter);
            }
            return hideDuplicates(ret);
        }

        public static FilterPolicy[] getFilterPolicies()
        {
            List<FilterPolicy> ret = new List<FilterPolicy>();
            string result = exec("show policy all format=table");
            if (result.Contains("IPsec[05072]"))
                return new FilterPolicy[0];
            string[] parts = result.Replace("\r", "").Split('\n');
            for (int i = 4; i < parts.Length - 5; i++)
            {
                string part = parts[i];
                string[] cells = part.Split('\t');
                FilterPolicy filterPolicy = new FilterPolicy();
                filterPolicy.name = cells[0].Trim();
                filterPolicy.rules = int.Parse(cells[1].Trim());
                filterPolicy.assigned = true; //fuck windows nl
                ret.Add(filterPolicy);
            }
            return ret.ToArray();
        }

        private static Filter[] hideDuplicates(List<Filter> list) {
            List<Filter> ret = new List<Filter>();
            foreach (Filter filter in list)
            {
                if (!filtersContains(ret.ToArray(), filter.address))
                    ret.Add(filter);
            }
            return ret.ToArray();
        }

        public static FilterList getFilterList(string name)
        {
            FilterList[] filterLists = IPsec.getFilterLists();
            foreach (FilterList filterList in filterLists)
                if (filterList.name == name)
                    return filterList;
            return new FilterList();
        }

        public static FilterAction getFilterAction(string name)
        {
            FilterAction[] filterActions = IPsec.getFilterActions();
            foreach (FilterAction filterAction in filterActions)
                if (filterAction.name == name)
                    return filterAction;
            return new FilterAction();
        }

        public static FilterPolicy getFilterPolicy(string name)
        {
            FilterPolicy[] filterPolicies = IPsec.getFilterPolicies();
            foreach (FilterPolicy filterPolicy in filterPolicies)
                if (filterPolicy.name == name)
                    return filterPolicy;
            return new FilterPolicy();
        }

        private static bool filtersContains(IPsec.Filter[] filters, IPAddress address)
        {
            foreach (IPsec.Filter filter in filters)
                if (filter.address.ToString() == address.ToString())
                    return true;
            return false;
        }


        public static void addFilterList(string name, string description = "")
        {
            exec("add filterlist name=\"" + name + "\" description=\"" + description + "\"");
        }

        public static void addAction(string name, string action, string description = "")
        {
            exec("add filteraction name=\"" + name + "\" action=\"" + action + "\" description=\"" + description + "\"");
        }

        public static void addFilter(FilterList list, string src, string dst = "Me")
        {
            string s = exec("add filter filterlist=\"" + list.name + "\" srcaddr=\"" + src + "\" dstaddr=\"" + dst + "\"");
        }

        public static void deleteFilter(FilterList list, string src, string dst = "Me")
        {
            exec("delete filter filterlist=\"" + list.name + "\" srcaddr=\"" + src + "\" dstaddr=\"" + dst + "\"");
        }

        public static void addFilterPolicy(FilterList list, string name, string description="", bool assign = true, int pollinginterval = 30)
        {
            exec("add policy name=\"" + name + "\" description=\"" + description + "\" assign=\"" + (assign ? "yes" : "no") + "\" pollinginterval=\"" + pollinginterval + "\"");
        }

        public static void addFilterRule(string name, FilterPolicy policy, FilterList list, FilterAction action)
        {
            string s = exec("add rule name=\"" + name + "\" policy=\"" + policy.name + "\" filterlist=\"" + list.name + "\" filteraction=\"" + action.name + "\"");
        }

        private static string exec(string cmd)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh");
            psi.Arguments = "ipsec static " + cmd;

            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;

            psi.UseShellExecute = false;
            psi.Verb = "runas";

            psi.RedirectStandardOutput = true;

            Process p = Process.Start(psi);
            string ret = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            return ret;
        }
    }
}
