using System;
using System.Linq;
using System.Collections.Generic;

namespace GW_1
{
    public enum ConnectorDirection
    {
        ToRightPE,
        FromRightPE,
        ToBottomPE,
        FromBottomPE
    }

    public class DataFlowConnector
    {
        public string DataFlowName { get; }
        public ConnectorDirection FlowDirection { get; }

        public DataFlowConnector(string dataFlowName, ConnectorDirection flowDirection)
        {
            DataFlowName = dataFlowName;
            FlowDirection = flowDirection;
        }
    }
}
