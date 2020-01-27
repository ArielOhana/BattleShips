using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Server
{
    class User
    {
        private const string FILENAME = "log.txt";
        private string username;
        private int wins;
        private int loses;
        private int rank;
        private int ID;
        public User(string username, int usernameline, int ID)
        {
            this.ID = ID;
            this.username = username;
            string[] CodeLines = File.ReadAllLines("log.txt");
            string WLR = CodeLines[usernameline + 2];
            int firstcut = WLR.IndexOf("/");
            int secondcut = WLR.LastIndexOf("/") - 1;
            this.rank = int.Parse(WLR.Substring(0, firstcut));
            this.wins = int.Parse(WLR.Substring(firstcut+1,secondcut - firstcut));
            this.loses = int.Parse(WLR.Substring(secondcut+2, WLR.Length - (2 + secondcut)));
        }
        public static void lineChanger(string newText, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(FILENAME);
            arrLine[line_to_edit - 1] = newText;
            File.WriteAllLines(FILENAME, arrLine);
        }
        public int GetW()
        {
            return this.wins;
        }
        public int GetL()
        {
            return this.loses;
        }
        public int GetR()
        {
            return this.rank;
        }
        public string GetUN()
        {
            return this.username;
        }
    }
}
