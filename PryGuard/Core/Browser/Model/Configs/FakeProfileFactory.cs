using System;
using System.Text;
using System.Linq;
using System.Globalization;

using System.ComponentModel;
using System.Collections.Generic;
using System.Security.Cryptography;
using CefSharp;
using PryGuard.Core.Browser.Settings;
using PryGuard.Resources.Helpers;
using PryGuard.Core.Browser.Model.Configs;
using PryGuard.DataModels;

namespace PryGuard.Core.Browser.Model.Configs;
public class FakeProfileFactory
{
    private static readonly Dictionary<OSVersion, string> OsVersions = new Dictionary<OSVersion, string>()
        {
            {OSVersion.Windows7, "Windows NT 6.1"}, {OSVersion.Windows8, "Windows NT 6.2"},
            {OSVersion.Windows81, "Windows NT 6.3"}, {OSVersion.Windows10, "Windows NT 10.0"},{OSVersion.Windows11, "Windows NT 10.0.22000"}
        };

    private static readonly List<string> ChromeBuildVersion = new List<string>()
        {
            "128.0.6613.11","128.0.6613.10","128.0.6613.9","128.0.6613.8","128.0.6613.7","128.0.6613.6","128.0.6613.5","128.0.6613.4","128.0.6613.3","128.0.6613.2",
            "128.0.6613.1","128.0.6613.0","128.0.6612.3","128.0.6612.2","128.0.6611.2","128.0.6612.1","128.0.6612.0","128.0.6611.1","128.0.6611.0","128.0.6610.1",
            "128.0.6610.0","128.0.6609.1","128.0.6609.0","128.0.6608.1","128.0.6608.0","128.0.6607.1","128.0.6607.0","128.0.6606.1","128.0.6606.0","128.0.6605.3",
            "128.0.6605.2","128.0.6605.1","128.0.6605.0","128.0.6604.1","128.0.6604.0","128.0.6603.1","128.0.6603.0","128.0.6602.2","128.0.6601.2","128.0.6602.1",
            "128.0.6602.0","128.0.6601.1","128.0.6601.0","128.0.6598.2","128.0.6600.1","128.0.6600.0","128.0.6599.1","128.0.6599.0","128.0.6598.1","128.0.6598.0",
            "128.0.6597.1","128.0.6597.0","128.0.6596.1","128.0.6596.0","128.0.6595.1","128.0.6595.0","128.0.6594.1","128.0.6594.0","128.0.6593.1","128.0.6593.0",
            "128.0.6592.1","128.0.6592.0","128.0.6591.1","128.0.6591.0","128.0.6590.1","128.0.6590.0","128.0.6589.1","128.0.6589.0","128.0.6588.2","128.0.6588.1",
            "128.0.6588.0","128.0.6587.1","128.0.6587.0","128.0.6586.1","128.0.6586.0","128.0.6585.1","128.0.6585.0","128.0.6584.1","128.0.6584.0","128.0.6583.1",
            "128.0.6583.0","128.0.6582.1","128.0.6582.0","128.0.6581.1","128.0.6581.0","128.0.6580.1","128.0.6580.0","128.0.6579.1","128.0.6579.0","128.0.6578.1",
            "128.0.6578.0","128.0.6577.1","128.0.6577.0","128.0.6576.1","128.0.6576.0","128.0.6575.1","128.0.6575.0","128.0.6574.1","128.0.6574.0","128.0.6573.1",
            "128.0.6573.0","128.0.6572.1","128.0.6572.0","128.0.6571.1","128.0.6571.0","128.0.6570.1","128.0.6570.0","128.0.6557.7","128.0.6557.8","128.0.6557.9",
            "128.0.6569.1","128.0.6569.0","128.0.6568.1","128.0.6568.0","128.0.6567.1","128.0.6567.0","128.0.6566.1","128.0.6566.0","128.0.6565.1","128.0.6565.0",
            "128.0.6563.2","128.0.6561.4","128.0.6564.1","128.0.6564.0","128.0.6563.1","128.0.6563.0","128.0.6561.3","128.0.6561.2","128.0.6562.1","128.0.6562.0",
            "128.0.6561.1","128.0.6561.0","128.0.6557.3","128.0.6557.4","128.0.6557.5","128.0.6557.6","128.0.6557.2","128.0.6560.1","128.0.6560.0","128.0.6559.0",
            "128.0.6558.0","128.0.6557.1","128.0.6557.0","128.0.6556.2","128.0.6555.2","128.0.6556.1","128.0.6556.0","128.0.6555.1","128.0.6555.0","128.0.6554.1",
            "128.0.6554.0","128.0.6553.1","128.0.6553.0","128.0.6552.1","128.0.6552.0","128.0.6550.2","128.0.6551.1","128.0.6551.0","128.0.6550.1","128.0.6550.0",
            "128.0.6549.1","128.0.6549.0","128.0.6547.3","128.0.6548.1","128.0.6548.0","128.0.6547.2","128.0.6547.1","128.0.6547.0","128.0.6546.1","128.0.6546.0",
            "128.0.6545.3","128.0.6545.2","128.0.6545.1","128.0.6545.0","128.0.6544.1","128.0.6544.0","128.0.6543.1","128.0.6543.0","128.0.6542.1","128.0.6542.0",
            "128.0.6541.1","128.0.6541.0","128.0.6540.4","128.0.6540.3","128.0.6540.2","128.0.6540.1","128.0.6540.0","128.0.6539.1","128.0.6539.0","128.0.6538.1",
            "128.0.6538.0","128.0.6537.3","128.0.6535.2","128.0.6537.2","128.0.6534.3","128.0.6537.1","128.0.6537.0","128.0.6536.1","128.0.6536.0","128.0.6534.2",
            "128.0.6535.1","128.0.6535.0","128.0.6534.1","128.0.6534.0","127.0.6533.82","127.0.6533.81","127.0.6533.80","127.0.6533.79","127.0.6533.78","127.0.6533.77",
            "127.0.6533.76","127.0.6533.73","127.0.6533.74","127.0.6533.75","127.0.6533.72","127.0.6533.69","127.0.6533.70","127.0.6533.71","127.0.6533.68","127.0.6533.65",
            "127.0.6533.66","127.0.6533.67","127.0.6533.64","127.0.6533.63","127.0.6533.62","127.0.6533.61","127.0.6533.60","127.0.6533.59","127.0.6533.58","127.0.6533.57",
            "127.0.6533.56","127.0.6533.52","127.0.6533.53","127.0.6533.54","127.0.6533.55","127.0.6533.51","127.0.6533.50","127.0.6533.49","127.0.6533.48","127.0.6533.47",
            "127.0.6533.46","127.0.6533.45","127.0.6533.44","127.0.6533.43","127.0.6533.42","127.0.6533.41","127.0.6533.40","127.0.6533.39","127.0.6533.38","127.0.6533.37",
            "127.0.6533.36","127.0.6533.35","127.0.6533.34","127.0.6533.33","127.0.6533.32","127.0.6533.31","127.0.6533.30","127.0.6533.29","127.0.6533.28","127.0.6533.27",
            "127.0.6533.26","127.0.6533.25","127.0.6533.24","127.0.6533.23","127.0.6533.22","127.0.6533.21","127.0.6533.20","127.0.6533.19","127.0.6533.18","127.0.6533.17",
            "127.0.6533.16","127.0.6533.15","127.0.6533.14","127.0.6533.13","127.0.6533.12","127.0.6533.11","127.0.6533.10","127.0.6533.9","127.0.6515.10","127.0.6533.8",
            "127.0.6528.2","127.0.6529.2","127.0.6533.5","127.0.6533.4","127.0.6533.3","127.0.6533.2","127.0.6525.3","127.0.6525.2","127.0.6533.1","127.0.6533.0",
            "127.0.6532.1","127.0.6532.0","127.0.6490.3","127.0.6531.1","127.0.6531.0","127.0.6530.1","127.0.6530.0","127.0.6529.1","127.0.6529.0","127.0.6528.1",
            "127.0.6528.0","127.0.6527.1","127.0.6527.0","127.0.6523.4","127.0.6526.1","127.0.6526.0","127.0.6525.1","127.0.6525.0","127.0.6523.3","127.0.6521.2",
            "127.0.6524.1","127.0.6524.0","127.0.6523.2","127.0.6523.1","127.0.6523.0","127.0.6515.9","127.0.6517.2","127.0.6522.1","127.0.6522.0","127.0.6515.6",
            "127.0.6521.1","127.0.6521.0","127.0.6515.2","127.0.6520.1","127.0.6520.0","127.0.6519.1","127.0.6519.0","127.0.6518.2","127.0.6518.1","127.0.6518.0",
            "127.0.6517.1","127.0.6517.0","127.0.6516.3","127.0.6516.2","127.0.6516.1","127.0.6516.0","127.0.6515.1","127.0.6515.0","127.0.6514.1","127.0.6514.0",
            "127.0.6512.2","127.0.6512.1","127.0.6512.0","127.0.6510.5","127.0.6510.4","127.0.6510.3","127.0.6510.2","127.0.6511.1","127.0.6511.0","127.0.6510.1",
            "127.0.6510.0","127.0.6509.1","127.0.6509.0","127.0.6508.1","127.0.6508.0","127.0.6507.3","127.0.6507.2","127.0.6507.1","127.0.6507.0","127.0.6506.2",
            "127.0.6506.1","127.0.6506.0","127.0.6505.1","127.0.6505.0","127.0.6504.1","127.0.6504.0","127.0.6503.1","127.0.6503.0","127.0.6502.1","127.0.6502.0",
            "127.0.6496.3","127.0.6501.2","127.0.6501.1","127.0.6501.0","127.0.6500.1","127.0.6500.0","127.0.6496.2","127.0.6499.4","127.0.6499.3","127.0.6499.2",
            "127.0.6499.1","127.0.6499.0","127.0.6498.3","127.0.6497.2","127.0.6498.2","127.0.6498.1","127.0.6498.0","127.0.6497.1","127.0.6497.0","127.0.6496.1",
            "127.0.6496.0","127.0.6495.1","127.0.6495.0","127.0.6494.1","127.0.6494.0","127.0.6493.2","127.0.6493.1","127.0.6493.0","127.0.6492.1","127.0.6492.0",
            "127.0.6491.1","127.0.6491.0","127.0.6490.2","127.0.6490.1","127.0.6490.0","127.0.6489.1","127.0.6489.0","127.0.6488.1","127.0.6488.0","127.0.6487.1",
            "127.0.6487.0","127.0.6486.1","127.0.6486.0","127.0.6485.1","127.0.6485.0","127.0.6484.1","127.0.6484.0","127.0.6483.0","126.0.6478.224","126.0.6478.223",
            "126.0.6478.222","126.0.6478.221","126.0.6478.193","126.0.6478.220","126.0.6478.219","126.0.6478.218","126.0.6478.217","126.0.6478.216","126.0.6478.215","126.0.6478.214",
            "126.0.6478.213","126.0.6478.192","126.0.6478.136","126.0.6478.137","126.0.6478.138","126.0.6478.191","126.0.6478.187","126.0.6478.188","126.0.6478.189","126.0.6478.190",
            "126.0.6478.186","126.0.6478.183","126.0.6478.184","126.0.6478.185","126.0.6478.182","126.0.6478.181","126.0.6478.180","126.0.6478.179","126.0.6478.178","126.0.6478.177",
            "126.0.6478.176","126.0.6478.175","126.0.6478.174","126.0.6478.173","126.0.6478.172","126.0.6478.171","126.0.6478.170","126.0.6478.169","126.0.6478.168","126.0.6478.167",
            "126.0.6478.166","126.0.6478.165","126.0.6478.156","126.0.6478.164","126.0.6478.163","126.0.6478.162","126.0.6478.161","126.0.6478.160","126.0.6478.155","126.0.6478.154",
            "126.0.6478.153","126.0.6478.135","126.0.6478.134","126.0.6478.133","126.0.6478.132","126.0.6478.131","126.0.6478.130","126.0.6478.127","126.0.6478.128","126.0.6478.129",
            "126.0.6478.126","126.0.6478.123","126.0.6478.124","126.0.6478.125","126.0.6478.122","126.0.6478.121","126.0.6478.120","126.0.6478.119","126.0.6478.118","126.0.6478.115",
            "126.0.6478.116","126.0.6478.117","126.0.6478.111","126.0.6478.112","126.0.6478.113","126.0.6478.114","126.0.6478.110","126.0.6478.107","126.0.6478.108","126.0.6478.109",
            "126.0.6478.106","126.0.6478.105","126.0.6478.104","126.0.6478.103","126.0.6478.102","126.0.6478.101","126.0.6478.62","126.0.6478.63","126.0.6478.64","126.0.6478.72",
            "126.0.6478.73","126.0.6478.74","126.0.6478.61","126.0.6478.71","126.0.6478.60","126.0.6478.59","126.0.6478.56","126.0.6478.57","126.0.6478.58","126.0.6478.55",
            "126.0.6478.54","126.0.6478.51","126.0.6478.52","126.0.6478.53","126.0.6478.50","126.0.6478.49","126.0.6478.48","126.0.6478.47","126.0.6478.46","126.0.6478.45",
            "126.0.6478.41","126.0.6478.42","126.0.6478.43","126.0.6478.44","126.0.6478.37","126.0.6478.38","126.0.6478.39","126.0.6478.40","126.0.6478.36","126.0.6478.35",
            "126.0.6478.34","126.0.6478.33","126.0.6478.32","126.0.6478.31","126.0.6478.30","126.0.6478.29","126.0.6478.28","126.0.6478.27","126.0.6478.26","126.0.6478.25",
            "126.0.6478.24","126.0.6478.23","126.0.6478.22","126.0.6478.21","126.0.6478.20","126.0.6478.19","126.0.6478.18","126.0.6478.17","126.0.6478.16","126.0.6478.15",
            "126.0.6478.14","126.0.6478.13","126.0.6478.12","126.0.6478.11","126.0.6478.10","126.0.6478.9","126.0.6478.8","126.0.6478.7","126.0.6478.6","126.0.6478.5",
            "126.0.6478.4","126.0.6478.3","126.0.6478.2","126.0.6478.1","126.0.6478.0","126.0.6477.4","126.0.6477.3","126.0.6477.2","126.0.6477.1","126.0.6477.0",
            "126.0.6476.1","126.0.6476.0","126.0.6475.1","126.0.6475.0","126.0.6474.1","126.0.6474.0","126.0.6473.1","126.0.6473.0","126.0.6472.1","126.0.6472.0",
            "126.0.6468.2","126.0.6471.1","126.0.6471.0","126.0.6470.1","126.0.6470.0","126.0.6469.2","126.0.6469.1","126.0.6469.0","126.0.6468.1","126.0.6468.0",
            "126.0.6467.2","126.0.6467.1","126.0.6467.0","126.0.6466.1","126.0.6466.0","126.0.6465.2","126.0.6465.1","126.0.6465.0","126.0.6463.3","126.0.6464.1",
            "126.0.6464.0","126.0.6463.2","126.0.6463.1","126.0.6463.0","126.0.6462.2","126.0.6462.1","126.0.6462.0","126.0.6461.1","126.0.6461.0","126.0.6460.1",
            "126.0.6460.0","126.0.6459.1","126.0.6459.0","126.0.6458.1","126.0.6458.0","126.0.6457.1","126.0.6457.0","126.0.6456.1","126.0.6456.0","126.0.6452.7",
            "126.0.6455.2","126.0.6452.6","126.0.6452.5","126.0.6455.1","126.0.6455.0","126.0.6454.2","126.0.6452.4","126.0.6452.3","126.0.6454.1","126.0.6454.0",
            "126.0.6452.2","126.0.6453.1","126.0.6453.0","126.0.6452.1","126.0.6452.0","126.0.6447.3","126.0.6451.1","126.0.6451.0","126.0.6450.1","126.0.6450.0",
            "126.0.6447.2","126.0.6449.2","126.0.6449.1","126.0.6449.0","126.0.6448.2","126.0.6448.1","126.0.6448.0","126.0.6447.1","126.0.6447.0","126.0.6446.1",
            "126.0.6446.0","126.0.6445.1","126.0.6445.0","126.0.6444.1","126.0.6444.0","126.0.6443.1","126.0.6443.0","126.0.6442.1","126.0.6442.0","126.0.6441.2",
            "126.0.6441.1","126.0.6441.0","126.0.6440.1","126.0.6440.0","126.0.6425.4","126.0.6439.1","126.0.6439.0","126.0.6437.4","126.0.6438.3","126.0.6436.5",
            "126.0.6437.3","126.0.6438.2","126.0.6438.1","126.0.6438.0","126.0.6437.2","126.0.6437.1","126.0.6437.0","126.0.6436.4","126.0.6436.3","126.0.6436.2",
            "126.0.6436.1","126.0.6436.0","126.0.6435.1","126.0.6435.0","126.0.6434.1","126.0.6434.0","126.0.6433.1","126.0.6433.0","126.0.6432.1","126.0.6432.0",
            "126.0.6431.1","126.0.6431.0","126.0.6430.1","126.0.6430.0","126.0.6429.1","126.0.6429.0","126.0.6428.2","126.0.6428.1","126.0.6428.0","126.0.6427.3",
            "126.0.6427.2","126.0.6427.1","126.0.6427.0","126.0.6425.3","126.0.6425.2","126.0.6423.2","126.0.6426.1","126.0.6426.0","126.0.6425.1","126.0.6425.0",
            "126.0.6424.2","126.0.6424.1","126.0.6424.0","126.0.6423.1","126.0.6423.0",
        };
    private static readonly List<string> ChromeBuildVersionWin_7_8_81 = new List<string>()
        {

            "109.0.5414.0", "109.0.5414.1", "109.0.5414.2", "109.0.5414.3", "109.0.5414.4", "109.0.5414.5", "109.0.5414.6", "109.0.5414.7", "109.0.5414.8", "109.0.5414.9",
            "109.0.5414.10", "109.0.5414.11", "109.0.5414.12", "109.0.5414.13", "109.0.5414.14", "109.0.5414.15", "109.0.5414.16", "109.0.5414.17", "109.0.5414.18", "109.0.5414.19",
            "109.0.5414.20", "109.0.5414.21", "109.0.5414.22", "109.0.5414.23", "109.0.5414.24", "109.0.5414.25", "109.0.5414.26", "109.0.5414.27", "109.0.5414.28", "109.0.5414.29",
            "109.0.5414.30", "109.0.5414.31", "109.0.5414.32", "109.0.5414.33", "109.0.5414.34", "109.0.5414.35", "109.0.5414.36", "109.0.5414.37", "109.0.5414.38", "109.0.5414.39",
            "109.0.5414.40", "109.0.5414.41", "109.0.5414.42", "109.0.5414.43", "109.0.5414.44", "109.0.5414.45", "109.0.5414.46", "109.0.5414.47", "109.0.5414.48", "109.0.5414.49",
            "109.0.5414.50", "109.0.5414.51", "109.0.5414.52", "109.0.5414.53", "109.0.5414.54", "109.0.5414.55", "109.0.5414.56", "109.0.5414.57", "109.0.5414.58", "109.0.5414.59",
            "109.0.5414.60", "109.0.5414.61", "109.0.5414.62", "109.0.5414.63", "109.0.5414.64", "109.0.5414.65", "109.0.5414.66", "109.0.5414.67", "109.0.5414.68", "109.0.5414.69",
            "109.0.5414.70", "109.0.5414.71", "109.0.5414.72", "109.0.5414.73", "109.0.5414.74", "109.0.5414.75", "109.0.5414.76", "109.0.5414.77", "109.0.5414.78", "109.0.5414.79",
            "109.0.5414.80", "109.0.5414.81", "109.0.5414.82", "109.0.5414.83", "109.0.5414.84", "109.0.5414.85", "109.0.5414.86", "109.0.5414.87", "109.0.5414.88", "109.0.5414.89",
            "109.0.5414.90", "109.0.5414.91", "109.0.5414.92", "109.0.5414.93", "109.0.5414.94", "109.0.5414.95", "109.0.5414.96", "109.0.5414.97", "109.0.5414.98", "109.0.5414.99",
            "109.0.5414.100", "109.0.5414.101", "109.0.5414.102", "109.0.5414.103", "109.0.5414.104", "109.0.5414.105", "109.0.5414.106", "109.0.5414.107", "109.0.5414.108", "109.0.5414.109",
            "109.0.5414.110", "109.0.5414.111", "109.0.5414.112", "109.0.5414.113", "109.0.5414.114", "109.0.5414.115", "109.0.5414.116", "109.0.5414.117", "109.0.5414.118", "109.0.5414.119",
            "109.0.5414.120", "109.0.5414.121", "109.0.5414.122", "109.0.5414.123", "109.0.5414.124", "109.0.5414.125", "109.0.5414.126", "109.0.5414.127", "109.0.5414.128", "109.0.5414.129",
            "109.0.5414.130", "109.0.5414.131", "109.0.5414.132", "109.0.5414.133", "109.0.5414.134", "109.0.5414.135", "109.0.5414.136", "109.0.5414.137", "109.0.5414.138", "109.0.5414.139",
            "109.0.5414.140", "109.0.5414.141", "109.0.5414.142", "109.0.5414.143", "109.0.5414.144", "109.0.5414.145", "109.0.5414.146", "109.0.5414.147", "109.0.5414.148", "109.0.5414.149",
            "109.0.5414.150", "109.0.5414.151", "109.0.5414.152", "109.0.5414.153", "109.0.5414.154", "109.0.5414.155", "109.0.5414.156", "109.0.5414.157", "109.0.5414.158", "109.0.5414.159",
            "109.0.5414.160", "109.0.5414.161", "109.0.5414.162", "109.0.5414.163", "109.0.5414.164", "109.0.5414.165", "109.0.5414.166", "109.0.5414.167", "109.0.5414.168", "109.0.5414.169",
            "109.0.5414.170", "109.0.5414.171", "109.0.5414.172", "109.0.5414.173", "109.0.5367.6", "109.0.5367.5", "109.0.5367.4", "109.0.5367.3", "109.0.5399.5", "109.0.5399.4",
            "109.0.5396.3", "109.0.5399.3", "109.0.5399.2", "109.0.5399.1", "109.0.5399.0", "109.0.5398.1", "109.0.5398.0", "109.0.5395.3", "109.0.5395.2", "109.0.5396.2",
            "109.0.5393.2", "109.0.5397.1", "109.0.5394.4", "109.0.5394.3", "109.0.5397.0", "109.0.5396.1", "109.0.5396.0", "109.0.5394.2", "109.0.5395.1", "109.0.5395.0",
            "109.0.5394.1", "109.0.5394.0", "109.0.5384.2", "109.0.5393.1", "109.0.5393.0", "109.0.5392.1", "109.0.5392.0", "109.0.5391.1", "109.0.5391.0", "109.0.5390.1",
            "109.0.5390.0", "109.0.5389.1", "109.0.5389.0", "109.0.5388.1", "109.0.5388.0", "109.0.5387.1", "109.0.5387.0", "109.0.5386.1", "109.0.5386.0", "109.0.5385.1",
            "109.0.5385.0", "109.0.5384.1", "109.0.5384.0", "109.0.5380.2", "109.0.5383.4", "109.0.5368.9", "109.0.5368.8", "109.0.5383.2", "109.0.5383.1", "109.0.5383.0",
            "109.0.5382.1", "109.0.5382.0", "109.0.5381.1", "109.0.5381.0", "109.0.5380.1", "109.0.5380.0", "109.0.5368.7", "109.0.5379.1", "109.0.5379.0", "109.0.5378.1",
            "109.0.5378.0", "109.0.5377.1", "109.0.5377.0", "109.0.5376.1", "109.0.5376.0", "109.0.5375.1", "109.0.5375.0", "109.0.5374.1", "109.0.5374.0", "109.0.5368.6",
            "109.0.5368.5", "109.0.5373.1", "109.0.5373.0", "109.0.5372.1", "109.0.5372.0", "109.0.5368.4", "109.0.5368.3", "109.0.5368.2", "109.0.5371.1", "109.0.5371.0",
            "109.0.5370.1", "109.0.5370.0", "109.0.5369.1", "109.0.5369.0", "109.0.5368.1", "109.0.5368.0", "109.0.5367.2", "109.0.5367.1", "109.0.5367.0", "109.0.5366.1",
            "109.0.5366.0", "109.0.5365.1", "109.0.5365.0", "109.0.5364.1", "109.0.5364.0", "109.0.5363.1", "109.0.5363.0", "109.0.5362.1", "109.0.5362.0", "109.0.5361.1",
            "109.0.5361.0", "109.0.5360.1", "109.0.5360.0"
        };


    public static List<ScreenSize> ScreenSizes { get; } = new List<ScreenSize>()
        {
            new ScreenSize(1920, 1080),
            new ScreenSize(1366, 768),
            new ScreenSize(1280, 1024),
            new ScreenSize(1440, 900),
            new ScreenSize(1600, 900),
            new ScreenSize(1280, 800),
            new ScreenSize(1024, 768),
            new ScreenSize(2560, 1440),
            new ScreenSize(3840, 2160)
        };

    public static List<int> CpuConcurrency { get; } = new List<int>() { 2, 4, 6, 8, 12 };
    public static List<int> MemoryAvailable { get; } = new List<int>() { 2, 4, 6, 8, 16 };

    private static HashSet<string> _allFonts = new HashSet<string>()
        {
            "AIGDT", "AMGDT", "AcadEref", "Adobe Arabic", "Adobe Caslon Pro", "Adobe Caslon Pro Bold",
            "Adobe Devanagari", "Adobe Fan Heiti Std B", "Adobe Fangsong Std R", "Adobe Garamond Pro",
            "Adobe Garamond Pro Bold", "Adobe Gothic Std B", "Adobe Hebrew", "Adobe Heiti Std R", "Adobe Kaiti Std R",
            "Adobe Ming Std L", "Adobe Myungjo Std M", "Adobe Naskh Medium", "Adobe Song Std L", "Agency FB", "Aharoni",
            "Alexandra Script", "Algerian", "Amadeus", "AmdtSymbols", "AnastasiaScript", "Andalus", "Angsana New",
            "AngsanaUPC", "Annabelle", "Aparajita", "Arabic Transparent", "Arabic Typesetting", "Arial", "Arial Baltic",
            "Arial Black", "Arial CE", "Arial CYR", "Arial Cyr", "Arial Greek", "Arial Narrow", "Arial Rounded MT Bold",
            "Arial TUR", "Arial Unicode MS", "Ariston", "Arno Pro", "Arno Pro Caption", "Arno Pro Display",
            "Arno Pro Light Display", "Arno Pro SmText", "Arno Pro Smbd", "Arno Pro Smbd Caption",
            "Arno Pro Smbd Display", "Arno Pro Smbd SmText", "Arno Pro Smbd Subhead", "Arno Pro Subhead",
            "BankGothic Lt BT", "BankGothic Md BT", "Baskerville Old Face", "Batang", "BatangChe", "Bauhaus 93",
            "Bell Gothic Std Black", "Bell Gothic Std Light", "Bell MT", "Berlin Sans FB", "Berlin Sans FB Demi",
            "Bernard MT Condensed", "Bickham Script One", "Bickham Script Pro Regular", "Bickham Script Pro Semibold",
            "Bickham Script Two", "Birch Std", "Blackadder ITC", "Blackoak Std", "Bodoni MT", "Bodoni MT Black",
            "Bodoni MT Condensed", "Bodoni MT Poster Compressed", "Book Antiqua", "Bookman Old Style",
            "Bookshelf Symbol 7", "Bradley Hand ITC", "Britannic Bold", "Broadway", "Browallia New", "BrowalliaUPC",
            "Brush Script MT", "Brush Script Std", "Calibri", "Calibri Light", "Californian FB", "Calisto MT",
            "Calligraph", "Cambria", "Cambria Math", "Candara", "Carolina", "Castellar", "Centaur", "Century",
            "Century Gothic", "Century Schoolbook", "Ceremonious Two", "Chaparral Pro", "Chaparral Pro Light",
            "Charlemagne Std", "Chiller", "CityBlueprint", "Clarendon BT", "Clarendon Blk BT", "Clarendon Lt BT",
            "Colonna MT", "Comic Sans MS", "CommercialPi BT", "CommercialScript BT", "Complex", "Consolas",
            "Constantia", "Cooper Black", "Cooper Std Black", "Copperplate Gothic Bold", "Copperplate Gothic Light",
            "Copyist", "Corbel", "Cordia New", "CordiaUPC", "CountryBlueprint", "Courier", "Courier New",
            "Courier New Baltic", "Courier New CE", "Courier New CYR", "Courier New Cyr", "Courier New Greek",
            "Courier New TUR", "Curlz MT", "DFKai-SB", "DaunPenh", "David", "Decor", "DejaVu Sans",
            "DejaVu Sans Condensed", "DejaVu Sans Light", "DejaVu Sans Mono", "DejaVu Serif", "DejaVu Serif Condensed",
            "DilleniaUPC", "DokChampa", "Dotum", "DotumChe", "Dutch801 Rm BT", "Dutch801 XBd BT", "Ebrima",
            "Eccentric Std", "Edwardian Script ITC", "Elephant", "Engravers MT", "Eras Bold ITC", "Eras Demi ITC",
            "Eras Light ITC", "Eras Medium ITC", "Estrangelo Edessa", "EucrosiaUPC", "Euphemia", "EuroRoman",
            "Eurostile", "FangSong", "Felix Titling", "Fixedsys", "Footlight MT Light", "Forte", "FrankRuehl",
            "Franklin Gothic Book", "Franklin Gothic Demi", "Franklin Gothic Demi Cond", "Franklin Gothic Heavy",
            "Franklin Gothic Medium", "Franklin Gothic Medium Cond", "Freehand521 BT", "FreesiaUPC", "Freestyle Script",
            "French Script MT", "Futura Md BT", "GDT", "GENISO", "Gabriola", "Gadugi", "Garamond", "Garamond Premr Pro",
            "Garamond Premr Pro Smbd", "Gautami", "Gentium Basic", "Gentium Book Basic", "Georgia", "Giddyup Std",
            "Gigi", "Gill Sans MT", "Gill Sans MT Condensed", "Gill Sans MT Ext Condensed Bold", "Gill Sans Ultra Bold",
            "Gill Sans Ultra Bold Condensed", "Gisha", "Gloucester MT Extra Condensed", "GothicE", "GothicG", "GothicI",
            "Goudy Old Style", "Goudy Stout", "GreekC", "GreekS", "Gulim", "GulimChe", "Gungsuh", "GungsuhChe",
            "Haettenschweiler", "Harlow Solid Italic", "Harrington", "Heather Script One", "Helvetica",
            "High Tower Text", "Hobo Std", "ISOCP", "ISOCP2", "ISOCP3", "ISOCPEUR", "ISOCT", "ISOCT2", "ISOCT3",
            "ISOCTEUR", "Impact", "Imprint MT Shadow", "Informal Roman", "IrisUPC", "Iskoola Pota", "Italic", "ItalicC",
            "ItalicT", "JasmineUPC", "Jokerman", "Juice ITC", "KaiTi", "Kalinga", "Kartika", "Khmer UI", "KodchiangUPC",
            "Kokila", "Kozuka Gothic Pr6N B", "Kozuka Gothic Pr6N EL", "Kozuka Gothic Pr6N H", "Kozuka Gothic Pr6N L",
            "Kozuka Gothic Pr6N M", "Kozuka Gothic Pr6N R", "Kozuka Gothic Pro B", "Kozuka Gothic Pro EL",
            "Kozuka Gothic Pro H", "Kozuka Gothic Pro L", "Kozuka Gothic Pro M", "Kozuka Gothic Pro R",
            "Kozuka Mincho Pr6N B", "Kozuka Mincho Pr6N EL", "Kozuka Mincho Pr6N H", "Kozuka Mincho Pr6N L",
            "Kozuka Mincho Pr6N M", "Kozuka Mincho Pr6N R", "Kozuka Mincho Pro B", "Kozuka Mincho Pro EL",
            "Kozuka Mincho Pro H", "Kozuka Mincho Pro L", "Kozuka Mincho Pro M", "Kozuka Mincho Pro R", "Kristen ITC",
            "Kunstler Script", "Lao UI", "Latha", "Leelawadee", "Letter Gothic Std", "Levenim MT",
            "Liberation Sans Narrow", "LilyUPC", "Lithos Pro Regular", "Lucida Bright", "Lucida Calligraphy",
            "Lucida Console", "Lucida Fax", "Lucida Handwriting", "Lucida Sans", "Lucida Sans Typewriter",
            "Lucida Sans Unicode", "MS Gothic", "MS Mincho", "MS Outlook", "MS PGothic", "MS PMincho",
            "MS Reference Sans Serif", "MS Reference Specialty", "MS Sans Serif", "MS Serif", "MS UI Gothic",
            "MT Extra", "MV Boli", "Magneto", "Maiandra GD", "Malgun Gothic", "Mangal", "Marlett",
            "Matura MT Script Capitals", "Meiryo", "Meiryo UI", "Mesquite Std", "Microsoft Himalaya",
            "Microsoft JhengHei", "Microsoft JhengHei UI", "Microsoft New Tai Lue", "Microsoft PhagsPa",
            "Microsoft Sans Serif", "Microsoft Tai Le", "Microsoft Uighur", "Microsoft YaHei", "Microsoft YaHei UI",
            "Microsoft Yi Baiti", "MingLiU", "MingLiU-ExtB", "MingLiU_HKSCS", "MingLiU_HKSCS-ExtB", "Minion Pro",
            "Minion Pro Cond", "Minion Pro Med", "Minion Pro SmBd", "Miriam", "Miriam Fixed", "Mistral", "Modern",
            "Modern No. 20", "Mongolian Baiti", "Monospac821 BT", "Monotxt", "Monotype Corsiva", "MoolBoran",
            "Myriad Arabic", "Myriad Hebrew", "Myriad Pro", "Myriad Pro Cond", "Myriad Pro Light", "Myriad Web Pro",
            "NSimSun", "Narkisim", "Niagara Engraved", "Niagara Solid", "Nirmala UI", "Nueva Std", "Nueva Std Cond",
            "Nyala", "OCR A Extended", "OCR A Std", "OCR-A BT", "OCR-B 10 BT", "Old English Text MT", "Onyx",
            "OpenSymbol", "Orator Std", "Ouverture script", "PMingLiU", "PMingLiU-ExtB", "Palace Script MT",
            "Palatino Linotype", "PanRoman", "Papyrus", "Parchment", "Perpetua", "Perpetua Titling MT",
            "Plantagenet Cherokee", "Playbill", "Poor Richard", "Poplar Std", "Prestige Elite Std", "Pristina",
            "Proxy 1", "Proxy 2", "Proxy 3", "Proxy 4", "Proxy 5", "Proxy 6", "Proxy 7", "Proxy 8", "Proxy 9", "Raavi",
            "Rage Italic", "Ravie", "Rockwell", "Rockwell Condensed", "Rockwell Extra Bold", "Rod", "Roman", "RomanC",
            "RomanD", "RomanS", "RomanT", "Romantic", "Rosewood Std Regular", "Sakkal Majalla", "SansSerif", "Script",
            "Script MT Bold", "ScriptC", "ScriptS", "Segoe Print", "Segoe Script", "Segoe UI", "Segoe UI Light",
            "Segoe UI Semibold", "Segoe UI Semilight", "Segoe UI Symbol", "Shonar Bangla", "Showcard Gothic", "Shruti",
            "SimHei", "SimSun", "SimSun-ExtB", "Simplex", "Simplified Arabic", "Simplified Arabic Fixed", "Small Fonts",
            "Snap ITC", "Square721 BT", "Stencil", "Stencil Std", "Stylus BT", "SuperFrench", "Swis721 BT",
            "Swis721 BdCnOul BT", "Swis721 BdOul BT", "Swis721 Blk BT", "Swis721 BlkCn BT", "Swis721 BlkEx BT",
            "Swis721 BlkOul BT", "Swis721 Cn BT", "Swis721 Ex BT", "Swis721 Hv BT", "Swis721 Lt BT", "Swis721 LtCn BT",
            "Swis721 LtEx BT", "Syastro", "Sylfaen", "Symap", "Symath", "Symbol", "Symeteo", "Symusic", "System",
            "Tahoma", "TeamViewer8", "Technic", "TechnicBold", "TechnicLite", "Tekton Pro", "Tekton Pro Cond",
            "Tekton Pro Ext", "Tempus Sans ITC", "Terminal", "Times New Roman", "Times New Roman Baltic",
            "Times New Roman CE", "Times New Roman CYR", "Times New Roman Cyr", "Times New Roman Greek",
            "Times New Roman TUR", "Traditional Arabic", "Trajan Pro", "Trebuchet MS", "Tunga", "Tw Cen MT",
            "Tw Cen MT Condensed", "Tw Cen MT Condensed Extra Bold", "Txt", "UniversalMath1 BT", "Utsaah", "Vani",
            "Verdana", "Vijaya", "Viner Hand ITC", "Vineta BT", "Vivaldi", "Vladimir Script", "Vrinda",
            "WP Arabic Sihafa", "WP ArabicScript Sihafa", "WP CyrillicA", "WP CyrillicB", "WP Greek Century",
            "WP Greek Courier", "WP Greek Helve", "WP Hebrew David", "WP MultinationalA Courier",
            "WP MultinationalA Helve", "WP MultinationalA Roman", "WP MultinationalB Courier",
            "WP MultinationalB Helve", "WP MultinationalB Roman", "WST_Czec", "WST_Engl", "WST_Fren", "WST_Germ",
            "WST_Ital", "WST_Span", "WST_Swed", "Webdings", "Wide Latin", "Wingdings", "Wingdings 2", "Wingdings 3",
            "ZWAdobeF"
        };

    private static HashSet<string> _fontsWin10 = new HashSet<string>()
        {
            "Arial", "Calibri", "Cambria", "Cambria Math", "Candara", "Comic Sans MS", "Comic Sans MS Bold",
            "Comic Sans", "Consolas", "Constantia", "Corbel", "Courier New", "Caurier Regular", "Ebrima",
            "Fixedsys Regular", "Franklin Gothic", "Gabriola Regular", "Gadugi", "Georgia",
            "HoloLens MDL2 Assets Regular", "Impact Regular", "Javanese Text Regular", "Leelawadee UI",
            "Lucida Console Regular", "Lucida Sans Unicode Regular", "Malgun Gothic", "Microsoft Himalaya Regular",
            "Microsoft JhengHei", "Microsoft JhengHei UI", "Microsoft PhangsPa", "Microsoft Sans Serif Regular",
            "Microsoft Tai Le", "Microsoft YaHei", "Microsoft YaHei UI", "Microsoft Yi Baiti Regular",
            "MingLiU_HKSCS-ExtB Regular", "MingLiu-ExtB Regular", "Modern Regular", "Mongolia Baiti Regular",
            "MS Gothic Regular", "MS PGothic Regular", "MS Sans Serif Regular", "MS Serif Regular",
            "MS UI Gothic Regular", "MV Boli Regular", "Myanmar Text", "Nimarla UI", "MV Boli Regular", "Myanmar Tet",
            "Nirmala UI", "NSimSun Regular", "Palatino Linotype", "PMingLiU-ExtB Regular", "Roman Regular",
            "Script Regular", "Segoe MDL2 Assets Regular", "Segoe Print", "Segoe Script", "Segoe UI",
            "Segoe UI Emoji Regular", "Segoe UI Historic Regular", "Segoe UI Symbol Regular", "SimSun Regular",
            "SimSun-ExtB Regular", "Sitka Banner", "Sitka Display", "Sitka Heading", "Sitka Small", "Sitka Subheading",
            "Sitka Text", "Small Fonts Regular", "Sylfaen Regular", "Symbol Regular", "System Bold", "Tahoma",
            "Terminal", "Times New Roman", "Trebuchet MS", "Verdana", "Webdings Regular", "Wingdings Regular",
            "Yu Gothic", "Yu Gothic UI", "Arial", "Arial Black", "Calibri", "Calibri Light", "Cambria", "Cambria Math",
            "Candara", "Comic Sans MS", "Consolas", "Constantia", "Corbel", "Courier", "Courier New", "Ebrima",
            "Fixedsys", "Franklin Gothic Medium", "Gabriola", "Gadugi", "Georgia", "HoloLens MDL2 Assets", "Impact",
            "Javanese Text", "Leelawadee UI", "Leelawadee UI Semilight", "Lucida Console", "Lucida Sans Unicode",
            "MS Gothic", "MS PGothic", "MS Sans Serif", "MS Serif", "MS UI Gothic", "MV Boli", "Malgun Gothic",
            "Malgun Gothic Semilight", "Marlett", "Microsoft Himalaya", "Microsoft JhengHei",
            "Microsoft JhengHei Light", "Microsoft JhengHei UI", "Microsoft JhengHei UI Light", "Microsoft New Tai Lue",
            "Microsoft PhagsPa", "Microsoft Sans Serif", "Microsoft Tai Le", "Microsoft YaHei", "Microsoft YaHei Light",
            "Microsoft YaHei UI", "Microsoft YaHei UI Light", "Microsoft Yi Baiti", "MingLiU-ExtB",
            "MingLiU_HKSCS-ExtB", "Modern", "Mongolian Baiti", "Myanmar Text", "NSimSun", "Nirmala UI",
            "Nirmala UI Semilight", "PMingLiU-ExtB", "Palatino Linotype", "Roman", "Script", "Segoe MDL2 Assets",
            "Segoe Print", "Segoe Script", "Segoe UI", "Segoe UI Black", "Segoe UI Emoji", "Segoe UI Historic",
            "Segoe UI Light", "Segoe UI Semibold", "Segoe UI Semilight", "Segoe UI Symbol", "SimSun", "SimSun-ExtB",
            "Sitka Banner", "Sitka Display", "Sitka Heading", "Sitka Small", "Sitka Subheading", "Sitka Text",
            "Small Fonts", "Sylfaen", "Symbol", "System", "Tahoma", "Terminal", "Times New Roman", "Trebuchet MS",
            "Verdana", "Webdings", "Wingdings", "Yu Gothic", "Yu Gothic Light", "Yu Gothic Medium", "Yu Gothic UI",
            "Yu Gothic UI Light", "Yu Gothic UI Semibold", "Yu Gothic UI Semilight"
        };

    private static HashSet<string> _fontsWin7 = new HashSet<string>()
        {
            "Aharoni Bold", "Andalus Regular", "Angsana New", "Angsana New Bold", "Angsana New Italic",
            "Angsana New Bold Italic", "AngsanaUPC", "AngsanaUPC Bold", "AngsanaUPC Italic", "AngsanaUPC Bold Italic",
            "Aparajita", "Aparajita Bold", "Aparajita Italic", "Aparajita Bold Italic", "Arabic Typesetting Regular",
            "Arial Unicode MS Regular", "Arial", "Arial Bold", "Arial Black", "Arial Italic", "Arial Bold Italic",
            "Batang", "BatangChe", "Browallia New", "Browallia New Bold", "Browallia New Italic",
            "Browallia New Bold Italic", "BrowalliaUPC", "BrowalliaUPC Bold", "BrowalliaUPC Italic",
            "BrowalliaUPC Bold Italic", "Calibri", "Calibri Bold", "Calibri Italic", "Calibri Bold Italic",
            "Cambria Math", "Cambria", "Cambria Bold", "Cambria Italic", "Cambria Bold Italic", "Candara",
            "Candara Bold", "Candara Italic", "Candara Bold Italic", "Comic Sans MS", "Comic Sans MS Bold", "Consolas",
            "Consolas Bold", "Consolas Italic", "Consolas Bold Italic", "Constantia", "Constantia Bold",
            "Constantia Italic", "Constantia Bold Italic", "Corbel", "Corbel Bold", "Corbel Italic",
            "Corbel Bold Italic", "Cordia New", "Cordia New Bold", "Cordia New Italic", "Cordia New Bold Italic",
            "CordiaUPC", "CordiaUPC Bold", "CordiaUPC Italic", "CordiaUPC Bold Italic", "Courier New",
            "Courier New Bold", "Courier New Italic", "Courier New Bold Italic", "DFKai-SB", "DaunPenh", "David",
            "David Bold", "DilleniaUPC", "DilleniaUPC Bold", "DilleniaUPC Italic", "DilleniaUPC Bold Italic",
            "DokChampa", "Dotum", "DotumChe", "Ebrima", "Ebrima Bold", "Estrangelo Edessa", "EucrosiaUPC",
            "EucrosiaUPC Bold", "EucrosiaUPC Italic", "EucrosiaUPC Bold Italic", "Euphemia", "FangSong", "FrankRuehl",
            "Franklin Gothic Medium", "Franklin Gothic Medium Italic", "FreesiaUPC", "FreesiaUPC Bold",
            "FreesiaUPC Italic", "FreesiaUPC Bold Italic", "Gabriola", "Gautami", "Gautami Bold", "Georgia",
            "Georgia Bold", "Georgia Italic", "& Georgia Bold Italic", "Gisha", "Gisha Bold", "Gulim", "GulimChe",
            "Gungsuh", "GungsuhChe", "Impact", "IrisUPC", "IrisUPC Bold", "IrisUPC Italic", "IrisUPC Bold Italic",
            "Iskoola Pota", "IskoolaPota Bold", "JasmineUPC", "JasmineUPC Bold", "JasmineUPC Italic",
            "JasmineUPC Bold Italic", "KaiTi", "Kalinga", "Kalinga Bold", "Kartika", "Kartika Bold", "Khmer UI",
            "Khmer UI Bold", "KodchiangUPC", "KodchiangUPC Bold", "KodchiangUPC Italic", "KodchiangUPC Bold Italic",
            "Kokila", "Kokila Bold", "Kokila Italic", "Kokila Bold Italic", "Lao UI", "Lao UI Bold", "Latha",
            "Latha Bold", "Leelawadee", "Leelawadee Bold", "Levenim MT", "Levenim MT Bold", "LilyUPC", "LilyUPC Bold",
            "LilyUPC Italic", "LilyUPC Bold Italic", "Lucida Console", "Lucida Sans Unicode", "MS Gothic", "MS Mincho",
            "MS PGothic", "MS PMincho", "MS UI Gothic", "MV Boli", "Malgun Gothic", "Malgun Gothic Bold", "Mangal",
            "Mangal Bold", "Meiryo UI", "Meiryo UI Bold", "Meiryo UI Italic", "Meiryo UI Bold Italic", "Meiryo",
            "Meiryo Bold", "Meiryo Italic", "Meiryo Bold Italic", "Microsoft Himalaya", "Microsoft JhengHei",
            "Microsoft JhengHei Bold", "Microsoft New Tai Lue", "Microsoft New Tai Lue Bold", "Microsoft PhagsPa",
            "Microsoft PhagsPa Bold", "Microsoft Sans Serif", "Microsoft Tai Le", "Microsoft Tai Le Bold",
            "Microsoft Uighur", "Microsoft YaHei", "Microsoft YaHei Bold", "Microsoft Yi Baiti", "MingLiU",
            "MingLiU-ExtB", "MingLiU_HKSCS", "MingLiU_HKSCS-ExtB", "Miriam", "Miriam Fixed", "Mongolian Baiti",
            "MoolBoran", "NSimSun", "Narkisim", "Nyala", "PMingLiU", "PMingLiU-ExtB", "Palatino Linotype",
            "Palatino Linotype Bold", "Palatino Linotype Italic", "Palatino Linotype Bold Italic",
            "Plantagenet Cherokee", "Raavi", "Raavi Bold", "Rod", "Sakkal Majalla", "Sakkal Majalla Bold",
            "Segoe Print", "Segoe Print Bold", "Segoe Script", "Segoe Script Bold", "Segoe UI Symbol", "Segoe UI",
            "Segoe UI Bold", "Segoe UI Italic", "Segoe UI Bold Italic", "Segoe UI Light", "Segoe UI Semibold",
            "Shonar Bangla", "Shonar Bangla Bold", "Shruti", "Shruti Bold", "SimHei", "SimSun", "SimSun-ExtB",
            "Simplified Arabic", "Simplified Arabic Bold", "Simplified Arabic Fixed", "Sylfaen", "Symbol", "Tahoma",
            "Tahoma Bold", "Times New Roman", " Times New Roman Bold", "Times New Roman Italic",
            "Times New Roman Bold Italic", "Traditional Arabic", "Traditional Arabic Bold", "Trebuchet MS",
            "Trebuchet MS Bold", "Trebuchet MS Italic", "Trebuchet MS Bold Italic", "Tunga", "Tunga Bold", "Utsaah",
            "Utsaah Bold", "Utsaah Italic", "Utsaah Bold Italic", "Vani", "Vani Bold", "Verdana", "Verdana Bold",
            "Verdana Italic", "Verdana Bold Italic", "Vijaya", "Vijaya Bold", "Vrinda", "Vrinda Bold", "Webdings",
            "Wingdings"
        };

    public static FakeProfile Generate()
    {
        bool isX64 = RandomHelper.GenerateRandomNumber(0, 2) == 0;
        FakeProfile result = new FakeProfile()
        {
            BrowserTypeType = GetAllEnumValues<BrowserType>(typeof(BrowserType)).GetRandomValue(),
            OsVersion = GetAllEnumValues<OSVersion>(typeof(OSVersion)).GetRandomValue(),
            IsX64 = isX64
        };
        result.IsSendDoNotTrack = true;
        result.HideCanvas = true;
        result.UserAgent = GenerateUserAgent(result);
        result.CpuConcurrency = CpuConcurrency.GetRandomValue();
        result.MemoryAvailable = result.CpuConcurrency != 12
            ? MemoryAvailable.Where(x => x >= result.CpuConcurrency).ToList().GetRandomValue()
            : 8;
        result.CanvasFingerPrintHash =
            GetMd5Hash(result.UserAgent + DateTime.Now.ToString(CultureInfo.InvariantCulture));
        result.BaseLatency = GenerateBaseLatencyValue();
        result.ChannelDataDelta = GenerateRandomDouble();
        result.ChannelDataIndexDelta = GenerateRandomDouble();
        result.FloatFrequencyDataDelta = GenerateRandomDouble();
        result.FloatFrequencyDataIndexDelta = GenerateRandomDouble();
        result.ChromeLanguageInfo = BrowserLanguageHelper.GetFullInfo(BrowserLanguageExtensions.GetValue(GenerateRandomInt(1, 2)));
        result.ScreenSize = ScreenSizes[GenerateRandomInt(0, ScreenSizes.Count - 1)];
        result.Fonts = GenerateAvailableFonts(result.OsVersion);
        result.WebGL = WebGLFactory.Generate();

        result.MediaDevicesSettings = new MediaDevicesSettings(GenerateRandomInt(0, 1), GenerateRandomInt(1, 3),
            GenerateRandomInt(0, 4));
        result.WebRtcSettings = new WebRTCSettings();
        result.GeoSettings = new GeoSettings();
        result.TimezoneSetting = new TimezoneSetting();


        return result;
    }

    public static FakeProfile Generate(Fingerprint botFingerprint)
    {
        var fakeProfile = Generate();
        fakeProfile.Fonts = botFingerprint.Fonts;
        fakeProfile.WebGL = WebGLFactory.Generate(botFingerprint);
        return fakeProfile;
    }

    private static List<string> GenerateAvailableFonts(OSVersion winVersion)
    {
        HashSet<string> source = new HashSet<string>();
        List<string> list = _allFonts.ToList();
        int count = list.Count;
        int int32 = Convert.ToInt32(count * 0.6);
        for (int index = 0; index < list.Count; ++index)
        {
            if (RandomHelper.GenerateRandomNumber(0, count) < int32)
                source.Add(list[index]);
        }

        return source.ToList();
    }

    public static double GenerateRandomDouble()
    {
        return RandomHelper.GenerateRandomNumber(1000, 9999) * 0.0001;
    }

    public static double GenerateRandomDouble(int from, int to)
    {
        return RandomHelper.GenerateRandomNumber(from, to);
    }

    public static int GenerateRandomInt(int from, int to)
    {
        return RandomHelper.GenerateRandomNumber(from, to);
    }

    public static string GenerateUserAgent(FakeProfile fakeProfile)
    {
        string baseUserAgent;

        if (fakeProfile.OsVersion == OSVersion.Windows10)
        {
            baseUserAgent = "Mozilla/5.0 (" +
                GetOSInfo(fakeProfile.OsVersion, fakeProfile.IsX64) +
                ") AppleWebKit/537.36 (KHTML, like Gecko) Chrome/" +
                ChromeBuildVersion.GetRandomValue() +
                " Safari/537.36";
        }
        else
        {
            baseUserAgent = "Mozilla/5.0 (" +
               GetOSInfo(fakeProfile.OsVersion, fakeProfile.IsX64) +
               ") AppleWebKit/537.36 (KHTML, like Gecko) Chrome/" +
               ChromeBuildVersionWin_7_8_81.GetRandomValue() +
               " Safari/537.36";
        }

        // Append the CefSharp browser version to the user agent
        return baseUserAgent + " /CefSharp Browser " + Cef.CefSharpVersion;
    }

    private static string GetX64String(bool isX64)
    {
        if (!isX64)
            return string.Empty;
        return "; Win64; x64";
    }

    private static string GetOSInfo(OSVersion osVersion, bool isX64)
    {
        return OsVersions[osVersion] + GetX64String(isX64);
    }

    private static List<T> GetAllEnumValues<T>(Type type)
    {
        if (type == null)
            throw new ArgumentNullException("type == null");
        if (!type.IsEnum)
            throw new InvalidEnumArgumentException(type.Name + " is not Enum");
        return Enum.GetValues(type).Cast<T>().ToList();
    }

    private static string GetMd5Hash(string value)
    {
        byte[] hash = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(value));
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte num in hash)
            stringBuilder.Append(num.ToString("x2"));
        return stringBuilder.ToString();
    }

    private static double GenerateBaseLatencyValue()
    {
        return double.Parse(
            string.Format("0,1{0}{1}{2}", RandomHelper.GenerateRandomNumber(0, 3), RandomHelper.GenerateRandomNumber(0, 99999),
                RandomHelper.GenerateRandomNumber(0, 9999999)), CultureInfo.GetCultureInfo("ru-Ru"));
    }
}