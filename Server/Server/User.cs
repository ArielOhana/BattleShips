using System.IO;

namespace Server
{
    class User
    {
        /// <summary>
        /// This class working with the database file!
        /// This class handles all the changes in the database file except of registering a new user.
        /// This class changing and sending ranks, wins and loses for each user it opens ((if nessesary))
        /// This class's objects are users from the database file.
        /// </summary>
        private const string FILENAME = "database.txt";
        private string username;
        private int wins;
        private int loses;
        private int rank;
        private int ID;
        private int firstcut;
        private int secondcut;
        private int usernameline;
        private string[] CodeLines;
        private string WLR;
        /// <summary>
        /// BUILDER - ejects the user's details and setting them up to be able to send easly.
        /// </summary>
        /// <param name="username"> the user username</param>
        /// <param name="usernameline"> the user username line in the database.</param>
        /// <param name="ID"> his defined ID.</param>
        public User(string username, int usernameline, int ID) 
        {
            this.usernameline = usernameline;
            this.ID = ID;
            this.username = username;
            this.CodeLines = File.ReadAllLines("database.txt");
            this.WLR = CodeLines[usernameline + 2];
            this.firstcut = WLR.IndexOf("/");
            this.secondcut = WLR.LastIndexOf("/") - 1;
            this.rank = int.Parse(WLR.Substring(0, firstcut));// cuts the details from the database file.
            this.wins = int.Parse(WLR.Substring(firstcut+1,secondcut - firstcut));// cuts the details from the database file.
            this.loses = int.Parse(WLR.Substring(secondcut+2, WLR.Length - (2 + secondcut))); // cuts the details from the database file.
        }
        /// <summary>
        /// Changing a line in the database file the receiving the new text and the line to edit.
        /// </summary>
        /// <param name="newText"> the new Text to set</param>
        /// <param name="line_to_edit"> the line to set.</param>
        public static void lineChanger(string newText, int line_to_edit)  
        {
            string[] arrLine = File.ReadAllLines(FILENAME);
            arrLine[line_to_edit - 1] = newText;
            File.WriteAllLines(FILENAME, arrLine);
        }
        /// <summary>
        /// this function adds a win to the user's details
        /// </summary>
        public void AddWin()
        {
            wins++;
            string towrite = rank + "/" + wins + "/" + loses;
            lineChanger(towrite, usernameline + 3);
        }
        /// <summary>
        /// this function adds a lose to the user's details
        /// </summary>
        public void AddLose() 
        {
            loses++;
            string towrite = rank + "/" + wins + "/" + loses;
            lineChanger(towrite, usernameline + 3);
        }
        /// <summary>
        /// this function sets the wins to the user's details
        /// </summary>
        /// <param name="wins"> .</param>
        public void SetWin(int wins) 
        {
            this.wins = wins;
            string towrite = rank + "/" + wins + "/" + loses;
            lineChanger(towrite, usernameline + 3);
        }
        /// <summary>
        /// this function sets the loses to the user's details
        /// </summary>
        /// <param name="loses">the loses ammount to set</param>
        public void SetLose(int loses) 
        {
            this.loses = loses;
            string towrite = rank + "/" + wins + "/" + loses;
            lineChanger(towrite, usernameline + 3);
        }
        /// <summary>
        /// this function sets the rank to the user's details
        /// </summary>
        /// <param name="rank">the rank to set</param>
        public void SetRank(int rank) 
        {
            this.rank = rank;
            string towrite = rank + "/" + wins + "/" + loses;
            lineChanger(towrite, usernameline + 3);
        }
        /// <summary>
        /// sends how many wins the user has
        /// </summary>
        /// <returns> the wins ammount</returns>
        public int GetW() 
        {
            return this.wins;
        }
        /// <summary>
        /// sends how many loses the user has
        /// </summary>
        /// <returns>the lose ammount</returns>
        public int GetL() 
        {
            return this.loses;
        }
        /// <summary>
        /// sends the rank of the user
        /// </summary>
        /// <returns>the rank</returns>
        public int GetR() 
        {
            return this.rank;
        }
        /// <summary>
        ///  sends the username of the user.
        /// </summary>
        /// <returns>the username</returns>
        public string GetUN()
        {
            return this.username;
        }
    }
}
