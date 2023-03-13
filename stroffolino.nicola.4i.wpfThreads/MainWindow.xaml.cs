using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nicola.stroffolino._4i.wpfThreads
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //const int GIRI = 1000;
        //int _counter = 0;
        //static readonly object _locker = new object();
        CountdownEvent semaforo { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        /*private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread thread1 = new Thread(incrementa1);
            thread1.Start();

            Thread thread2 = new Thread(incrementa2);
            thread2.Start();
        }

        // Processo lento che dobbiamo lanciare...
        private void incrementa1()
        {
            for (int x = 0; x < GIRI; x++)
            {
                lock (_locker)
                {
                    _counter++;
                }

                Dispatcher.Invoke(() => {
                    lblCounter1.Text = _counter.ToString();
                    pbrBar1.Value = _counter;
                });

                Thread.Sleep(1);
            }
        }

        private void incrementa2()
        {
            for (int x = 0; x < GIRI; x++)
            {
                lock (_locker)
                {
                    _counter++;
                }

                Dispatcher.Invoke(() => {
                    lblCounter2.Text = _counter.ToString();
                });

                Thread.Sleep(1);
            }
        }*/

        private void Start(object sender, RoutedEventArgs e) {
            StartBtn.IsEnabled = false;
            semaforo = new CountdownEvent(3);

            var thread1 = new Thread(Incrementa1);
            thread1.Start();

            var thread2 = new Thread(Incrementa2);
            thread2.Start();

            var thread3 = new Thread(Incrementa3);
            thread3.Start();

            var threadWait = new Thread(() => {
                semaforo.Wait();
                Dispatcher.Invoke(() => {
                    StartBtn.IsEnabled = true;
                });
            });
            threadWait.Start();
        }

        private void Incrementa1() {
            for (int i = 0; i <= 5000; i++) {
                //if (i % 2 != 0) continue; // Approssimare 
                Dispatcher.Invoke(() => {
                    lblCounter1.Text = i.ToString();
                    pbrBar1.Value = i;
                });
                Thread.Sleep(1);
            }

            semaforo.Signal();
        }

        private void Incrementa2() { 
            for (int i = 0; i <= 500; i++) {
                Dispatcher.Invoke(() => {
                    lblCounter2.Text = i.ToString();
                    pbrBar2.Value = i;
                });
                Thread.Sleep(10);
            }

            semaforo.Signal();
        }

        private void Incrementa3() {
            for (int i = 0; i <= 50; i++) {
                Dispatcher.Invoke(() => {
                    lblCounter3.Text = i.ToString();
                    pbrBar3.Value = i;
                });
                Thread.Sleep(100);
            }

            semaforo.Signal();
        }
    }
}
