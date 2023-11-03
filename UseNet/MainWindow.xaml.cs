using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Usenet
{
    public partial class MainWindow : Window
    {
        private NntpClient nntpClient;
        private string configFilePath = "config.txt";
        private List<string> groups;
        private string selectedGroup;

        public MainWindow()
        {
            InitializeComponent();

            // Initialiser NntpClient og læs konfiguration
            nntpClient = new NntpClient("news.dotsrc.org", 119); // 119 er portnummer for NNTP
            groups = new List<string>();
            LoadConfiguration();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Gem indtastet konfiguration til en fil
            string newsServer = newsServerTextBox.Text;
            string username = usernameTextBox.Text;
            string password = passwordBox.Password;

            File.WriteAllText(configFilePath, $"NewsServer={newsServer}\nUsername={username}\nPassword={password}");
        }

        private void LoadConfiguration()
        {
            if (File.Exists(configFilePath))
            {
                // Indlæs gemt konfiguration fra fil og udfyld tekst
                string[] lines = File.ReadAllLines(configFilePath);
                foreach (var line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string value = parts[1];

                        switch (key)
                        {
                            case "NewsServer":
                                newsServerTextBox.Text = value;
                                break;
                            case "Username":
                                usernameTextBox.Text = value;
                                break;
                            case "Password":
                                passwordBox.Password = value;
                                break;
                        }
                    }
                }
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Håndter knappen "Connect" klikket - forbind til NNTP-server
            string newsServer = newsServerTextBox.Text;
            string username = usernameTextBox.Text;
            string password = passwordBox.Password;

            if (nntpClient.Connect(username, password))
            {
                ServerNameLabel.Content = $"Forbundet til Usenet-server: {nntpClient.ServerName}";
                groups = nntpClient.ListGroups();
                groupsComboBox.ItemsSource = groups;
            }
            else
            {
                MessageBox.Show("Kunne ikke oprette forbindelse til Usenet-server.");
            }
        }

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            // Håndter knappen "List Groups" klikket - hent og vis liste over grupper
            groups = nntpClient.ListGroups();
            groupsComboBox.ItemsSource = groups;
        }

        private void DownloadGroupsButton_Click(object sender, RoutedEventArgs e)
        {
            // Håndter knappen "Download Groups" klikket
            if (groupsComboBox.SelectedItem != null)
            {
                string selectedGroup = groupsComboBox.SelectedItem.ToString();
                List<Article> articles = nntpClient.GetArticles(selectedGroup);

                // Downloadede artikler her
            }
            else
            {
                MessageBox.Show("Vælg venligst en gruppe, før du downloader artikler.");
            }
        }

        private void PostArticleButton_Click(object sender, RoutedEventArgs e)
        {
            
            string articleSubject = "Eksempel Emne";
            string articleText = "Dette er en eksempeltekst til artiklen.";

            bool success = nntpClient.PostArticle(selectedGroup, articleSubject, articleText);

            if (success)
            {
                MessageBox.Show("Artiklen er oprettet");
            }
            else
            {
                MessageBox.Show("Kunne ikke oprette artiklen.");
            }
        }

        private void SaveInterestButton_Click(object sender, RoutedEventArgs e)
        {
            if (groupsComboBox.SelectedItem != null)
            {
                string selectedGroup = groupsComboBox.SelectedItem.ToString();

                
            }
            else
            {
                MessageBox.Show("Vælg venligst en gruppe, inden du gemmer.");
            }
        }
    }
}
