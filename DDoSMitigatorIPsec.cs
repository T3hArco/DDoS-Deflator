using System;
using System.Collections.Generic;
using System.Text;

namespace netshWrapper
{
    class DDoSMitigatorIPsec
    {

        private static IPsec.FilterList ddosMitigationFilterList;
        private static IPsec.FilterAction ddosMitigationFilterAction;
        private static IPsec.FilterPolicy ddosMitigationFilterPolicy;

        public IPsec.FilterList DDoSMitigationFilterList
        {
            get
            {
                return ddosMitigationFilterList;
            }
        }

        public void init()
        {
            reloadFilterList();
            reloadFilterAction();
            reloadFilterPolicy();

            if (ddosMitigationFilterPolicy.rules == 1)
            {
                IPsec.addFilter(ddosMitigationFilterList, "1.1.1.1");
                IPsec.addFilterRule("ddosMitigationRule", ddosMitigationFilterPolicy, ddosMitigationFilterList, ddosMitigationFilterAction);
            }
        }

        public void reloadFilterList()
        {
            IPsec.FilterList[] filterLists = IPsec.getFilterLists();
            if (!filterListsContains(filterLists, "ddosMitigationList"))
            {
                IPsec.addFilterList("ddosMitigationList", "IP Filter List used by ddosMitigator");
            }
            ddosMitigationFilterList = IPsec.getFilterList("ddosMitigationList");
        }

        public void reloadFilterAction()
        {
            IPsec.FilterAction[] filterActions = IPsec.getFilterActions();
            if (!filterActionsContains(filterActions, "ddosMitigationBlock"))
            {
                IPsec.addAction("ddosMitigationBlock", "block", "IP Filter List used by ddosMitigator");
            }
            ddosMitigationFilterAction = IPsec.getFilterAction("ddosMitigationBlock");
        }

        public void reloadFilterPolicy()
        {
            IPsec.FilterPolicy[] filterPolicies = IPsec.getFilterPolicies();
            if (!filterPoliciesContains(filterPolicies, "ddosMitigationPolicy"))
            {
                IPsec.addFilterPolicy(ddosMitigationFilterList, "ddosMitigationPolicy", "Filter policy used by ddosMitigator");
            }
            ddosMitigationFilterPolicy = IPsec.getFilterPolicy("ddosMitigationPolicy");
        }

        private static bool filterListsContains(IPsec.FilterList[] filterLists, string name)
        {
            foreach (IPsec.FilterList filterList in filterLists)
                if (filterList.name == name)
                    return true;
            return false;
        }

        private static bool filterActionsContains(IPsec.FilterAction[] filterActions, string name)
        {
            foreach (IPsec.FilterAction filterAction in filterActions)
                if (filterAction.name == name)
                    return true;
            return false;
        }

        private static bool filterPoliciesContains(IPsec.FilterPolicy[] filterPolicies, string name)
        {
            foreach (IPsec.FilterPolicy filterPolicy in filterPolicies)
                if (filterPolicy.name == name)
                    return true;
            return false;
        }

        private static bool filterRulesContains(IPsec.FilterRule[] filterRules, bool enabled)
        {
            foreach (IPsec.FilterRule filterRule in filterRules)
                if (filterRule.enabled == enabled)
                    return true;
            return false;
        }

    }
}
