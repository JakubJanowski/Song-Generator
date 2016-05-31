using Engine;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApplication1 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            textBox.IsReadOnly = true;
            textBox2.IsReadOnly = true;

            Stream input = File.OpenRead(@"..\..\db.bin");
            WordDatabase.Load(input);
            input.Close();

            string path = @"..\..\songs.txt";
            string text = File.ReadAllText(path);
            MarkovChains.CreateMarkovChain(text);
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            button.IsEnabled = false;
            textBox.Text = "";
            Task.Run(() => {
                string lines;
                string previousWord, currentWord;
                string[] words;

                do
                    lines = MarkovChains.Generate(6);
                while (lines == null);

                Dispatcher.Invoke(() => {
                    textBox.Text += lines + '\n';
                });

                for (int i = 1; i < 15; i++) {
                    words = lines.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    previousWord = words[words.Length - 2];
                    currentWord = words[words.Length - 1];
                    
                    lines = MarkovChains.Generate(6, 0, previousWord, currentWord);
                    
                    while (lines == null)
                        lines = MarkovChains.Generate(6);

                    Dispatcher.Invoke(() => {
                        textBox.Text += lines + '\n';
                    });
                }

                Dispatcher.Invoke(() => {
                    button.IsEnabled = true;
                });
            });
        }

        private void button2_Click(object sender, RoutedEventArgs e) {
            button2.IsEnabled = false;
            string word = textBox3.Text;
            Task.Run(() => {
                var rhymingWords = WordDatabase.findRhymes(word);
                string formattedWords = "";
                foreach (var list in rhymingWords)
                    foreach (var rhyme in list)
                        if (!rhyme.Contains("("))
                            formattedWords += rhyme + "\n";
                Dispatcher.Invoke(() => {
                    textBox2.Text = formattedWords;
                    button2.IsEnabled = true;
                });
            });
        }
    }
}
