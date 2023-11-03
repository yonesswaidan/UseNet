using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Usenet
{
    public class NntpClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private string server;
        private int port;
        private string lastGroup;
        private string currentArticle;

        public string ServerName { get; private set; }

        public NntpClient(string server, int port)
        {
            this.server = server;
            this.port = port;
            ServerName = server;
        }

        public bool Connect(string username, string password)
        {
            client = new TcpClient();
            client.Connect(server, port);

            if (client.Connected)
            {
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.ASCII);
                writer = new StreamWriter(stream, Encoding.ASCII);

               
                string response = reader.ReadLine();
                if (!response.StartsWith("200", StringComparison.Ordinal))
                {
                    // Serveren reagerer ikke korrekt
                    return false;
                }

                // Authenticate 
                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    if (!Authenticate(username, password))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
        // Løbende kommer der de forskellige kommandoer alt fra list, post groups til quit

        public List<string> ListGroups()
        {
            SendCommand("LIST");
            string response = ReadResponse();
            List<string> groups = new List<string>();

            if (response.StartsWith("215", StringComparison.Ordinal))
            {
                // Process the list of newsgroups
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == ".")
                    {
                        break;
                    }
                    groups.Add(line);
                }
            }
            else
            {
               
            }

            return groups;
        }

        public List<Article> GetArticles(string groupName)
        {
            SendCommand($"GROUP {groupName}");
            string response = ReadResponse();

            if (response.StartsWith("211", StringComparison.Ordinal))
            {
                lastGroup = groupName;

                // Extract the number of articles available in the group
                string[] parts = response.Split(' ');
                int totalArticles = int.Parse(parts[2]);

                List<Article> articles = new List<Article>();
                for (int i = 1; i <= totalArticles; i++)
                {
                    articles.Add(new Article { ArticleNumber = i });
                }

                return articles;
            }

            return new List<Article>();
        }

        public string GetArticleContent(int articleNumber)
        {
            SendCommand($"ARTICLE {articleNumber}");
            string response = ReadResponse();
            string articleContent = "";

            if (response.StartsWith("220", StringComparison.Ordinal))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    articleContent += line + Environment.NewLine;

                    if (line == "." || line == "")
                    {
                        break;
                    }
                }
                currentArticle = articleContent;
            }

            return articleContent;
        }

        public bool PostArticle(string groupName, string subject, string content)
        {
            SendCommand($"GROUP {groupName}");
            string groupResponse = ReadResponse();

            if (groupResponse.StartsWith("211", StringComparison.Ordinal))
            {
                SendCommand("POST");
                string postResponse = ReadResponse();

                if (postResponse.StartsWith("340", StringComparison.Ordinal))
                {
                    SendArticleContent(subject, content);
                    return true;
                }
            }

            return false;
        }

        private void SendArticleContent(string subject, string content)
        {
            SendCommand($"Subject: {subject}");
            SendCommand("Newsgroups: " + lastGroup);
            SendCommand("Content-Type: text/plain; charset=us-ascii");
            SendCommand("");
            SendCommand(content);
            SendCommand(".");
            ReadResponse();
        }

        public void Disconnect()
        {
            if (lastGroup != null)
            {
                SendCommand("QUIT");
                lastGroup = null;
            }

            client.Close();
        }

        private bool Authenticate(string username, string password)
        {
            SendCommand($"AUTHINFO USER {username}");
            string response = ReadResponse();
            if (!response.StartsWith("381", StringComparison.Ordinal))
            {
                return false;
            }

            SendCommand($"AUTHINFO PASS {password}");
            response = ReadResponse();
            if (!response.StartsWith("281", StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        private void SendCommand(string command)
        {
            writer.WriteLine(command);
            writer.Flush();
        }

        private string ReadResponse()
        {
            StringBuilder response = new StringBuilder();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                response.AppendLine(line);
                if (Regex.IsMatch(line, @"^\d{3} .*")) // 3-digit code /Check off
                {
                    break;
                }
            }

            return response.ToString();
        }
    }
}
