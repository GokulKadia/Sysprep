using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SysprepUtility
{
    public class Utility
    {
    }
    public static class jsonRead
    {
        static Dictionary<string, string> UserDetails = new Dictionary<string, string>();
        private static List<string> getPayloadParamKeys(string Payload)
        {
            JObject o = JObject.Parse(Payload);
            JToken jt = o.SelectToken("$.sysprepSettings.parameters");
            return jt?.Select(x => x.ToString()).ToList();
        }
        public static Dictionary<string, string> Readjson(string JsonPay)
        {
            UserDetails = getPayloadParamKeys(JsonPay).Select(x =>
            {
                var itemArr = x.Split(new[] { ':' }, 2);
                var keyKVP = itemArr[0];
                keyKVP = keyKVP.Substring(1, keyKVP.Length - 2);
                var valKVP = itemArr[1]; return new { keyKVP, valKVP };
            }).ToDictionary(x => x.keyKVP, x =>
            {
                var val = x.valKVP.Split(':')[1];
                return val.Substring(val.IndexOf('"') + 1, val.LastIndexOf('"') - (val.IndexOf('"') + 1));
            });
            return UserDetails;
        }
    }
    public static class SysprepOperation
    {
        public static void RunSysprep(string sysprepfile)
        {
            try
            {
                if (Wow64Interop.EnableWow64FSRedirection(true) == true)
                {
                    Wow64Interop.EnableWow64FSRedirection(false);
                }

                Process Sysprep = new Process();
                Sysprep.StartInfo.FileName = "C:\\Windows\\System32\\Sysprep\\sysprep.exe";
                Sysprep.StartInfo.Arguments = $"/generalize /oobe /shutdown /unattend:{sysprepfile}";
                Sysprep.StartInfo.RedirectStandardOutput = true;
                Sysprep.StartInfo.UseShellExecute = false;
                Sysprep.StartInfo.CreateNoWindow = true;
                Sysprep.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Sysprep.Start();

                if (Wow64Interop.EnableWow64FSRedirection(false) == true)
                {
                    Wow64Interop.EnableWow64FSRedirection(true);
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }
        public static bool writeXMLFile(string path,Dictionary<string,string> UserDetails)
        {
            try
            {
                if (UserDetails.Count == 0)
                {
                    return false;
                }
                var doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList nodes = doc.GetElementsByTagName("Password");
                setNodes(nodes, "Password",UserDetails);
                nodes = doc.GetElementsByTagName("Username");
                setNodes(nodes, "UserName", UserDetails);
                nodes = doc.GetElementsByTagName("Description");
                setNodes(nodes, "UserName", UserDetails);
                nodes = doc.GetElementsByTagName("DisplayName");
                setNodes(nodes, "UserName", UserDetails);
                nodes = doc.GetElementsByTagName("Name");
                setNodes(nodes, "UserName", UserDetails);
                nodes = doc.GetElementsByTagName("TimeZone");
                setNodes(nodes, "TimeZone", UserDetails);
                doc.Save(path);
            }
            catch (Exception e) { }
            return true;
        }
        private static void setNodes(XmlNodeList nodes, string name, Dictionary<string, string> UserDetails)
        {
            foreach (XmlNode item in nodes)
            {
                if (name == "Password")
                    item.ChildNodes[0].InnerText = EncodeTo64(UserDetails.Where(x => x.Key == name).FirstOrDefault().Value);
                else
                    item.InnerText = UserDetails.Where(x => x.Key == name).FirstOrDefault().Value;
            }

        }
        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        //void xmlread()
        //{
        //    string PG = "HER";
        //    XmlDocument doc = new XmlDocument();
        //    doc.Load("C:\\Users\\Admin\\Desktop\\Sysprep.xml");
        //    string text = string.Empty;
        //    XmlNodeList xnl = doc.SelectNodes(string.Format("/unattend/settings/component[@name='{0}']/AutoLogon", "Microsoft-Windows-Shell-Setup"));
        //    foreach (XmlNode node in xnl)
        //    {
        //        text = node.Attributes["name"].InnerText;
        //        if (text == PG)
        //        {
        //            XmlNodeList xnl2 = doc.SelectNodes("/Periods/PeriodGroup/Period");
        //            foreach (XmlNode node2 in xnl2)
        //            {
        //                text = text + "<br>" + node2["PeriodName"].InnerText;
        //                text = text + "<br>" + node2["StartDate"].InnerText;
        //                text = text + "<br>" + node2["EndDate"].InnerText;
        //            }
        //        }

        //    }
        //}
    }  
}
/// <summary>
/// this class define for giving the rights permission for launching any
/// application from system32 because sysprep required some specific permission and rights 
/// </summary>
public class Wow64Interop
{
    [DllImport("Kernel32.Dll", EntryPoint = "Wow64EnableWow64FsRedirection")]
    public static extern bool EnableWow64FSRedirection(bool enable);
}