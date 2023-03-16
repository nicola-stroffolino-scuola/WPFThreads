using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nicola.stroffolino._4i.wpfThreads {
    // Source https://stackoverflow.com/a/67060100/17043535 Per l'MMWrapper
    // Source https://gist.github.com/mjs3339/b98bbf4075be0176ac521c9875652dfe Per il timeKillEvent
    public class WinMMWrapper {
        [DllImport("WinMM.dll", SetLastError = true)]
        public static extern uint timeSetEvent(int msDelay, int msResolution,
            TimerEventHandler handler, ref int userCtx, int eventType);
        [DllImport("WinMM.dll", SetLastError = true)]
        public static extern void timeKillEvent(UInt32 timerEventId);

        public delegate void TimerEventHandler(uint id, uint msg, ref int userCtx,
            int rsv1, int rsv2);

        public enum TimerEventType {
            OneTime = 0,
            Repeating = 1
        }

        private readonly Action _elapsedAction;
        private readonly int _elapsedMs;
        private readonly int _resolutionMs;
        private readonly TimerEventType _timerEventType;
        private readonly TimerEventHandler _timerEventHandler;

        public WinMMWrapper(int elapsedMs, int resolutionMs, TimerEventType timerEventType, Action elapsedAction) {
            _elapsedMs = elapsedMs;
            _resolutionMs = resolutionMs;
            _timerEventType = timerEventType;
            _elapsedAction = elapsedAction;
            _timerEventHandler = TickHandler;
        }

        public uint StartElapsedTimer() {
            var myData = 1; //dummy data
            return timeSetEvent(_elapsedMs, _resolutionMs / 10, _timerEventHandler, ref myData, (int)_timerEventType);
        }

        private void TickHandler(uint id, uint msg, ref int userctx, int rsv1, int rsv2) {
            _elapsedAction();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        //const int GIRI = 1000;
        //int _counter = 0;
        static readonly object Locker = new object();
        public int TotalCount { get; set; }
        CountdownEvent? Semaforo { get; set; }
        WinMMWrapper[] Timers { get; set; } // Dichiarati come attributi per evitare che il
                                            // Garbage Collector elimini le instanze di timer
                                            // al di fuori del suo scope
        public MainWindow() {
            InitializeComponent();
            TotalCount = 0;
            Timers = new WinMMWrapper[4];
        }

        private void Start(object sender, RoutedEventArgs e) {
            StartBtn.IsEnabled = false;
            Semaforo = new CountdownEvent(4);

            var thread1 = new Thread(Incrementa1);
            thread1.Start();

            var thread2 = new Thread(Incrementa2);
            thread2.Start();

            var thread3 = new Thread(Incrementa3);
            thread3.Start();

            var threadTot = new Thread(IncrementaTot);
            threadTot.Start();
            
            var threadWait = new Thread(() => {
                Semaforo.Wait();
                Dispatcher.Invoke(() => {
                    StartBtn.IsEnabled = true;
                });
            });
            threadWait.Start();
        }

        private void Incrementa1() {
            int i = 0;
            uint id = 0;
            Timers[0] = new WinMMWrapper(1, 0, WinMMWrapper.TimerEventType.Repeating, () => {
                lock (Locker) { TotalCount++; }
                i++;
                Dispatcher.Invoke(() => {
                    lblCounter1.Text = i.ToString();
                    pbrBar1.Value = i;
                });
                if (i == 5000) {
                    WinMMWrapper.timeKillEvent(id);
                    Semaforo!.Signal();
                }
            });
            id = Timers[0].StartElapsedTimer();
        }

        private void Incrementa2() {
            int i = 0;
            uint id = 0;
            Timers[1] = new WinMMWrapper(10, 0, WinMMWrapper.TimerEventType.Repeating, () => {
                lock (Locker) { TotalCount++; }
                i++;
                Dispatcher.Invoke(() => {
                    lblCounter2.Text = i.ToString();
                    pbrBar2.Value = i;
                });
                if (i == 500) {
                    WinMMWrapper.timeKillEvent(id);
                    Semaforo!.Signal();
                }
            });
            id = Timers[1].StartElapsedTimer();
        }

        private void Incrementa3() {
            int i = 0;
            uint id = 0;
            Timers[2] = new WinMMWrapper(100, 0, WinMMWrapper.TimerEventType.Repeating, () => {
                lock (Locker) { TotalCount++; }
                i++;
                Dispatcher.Invoke(() => {
                    lblCounter3.Text = i.ToString();
                    pbrBar3.Value = i;
                });
                if (i == 50) {
                    WinMMWrapper.timeKillEvent(id);
                    Semaforo!.Signal();
                }
            });
            id = Timers[2].StartElapsedTimer();
        }

        private void IncrementaTot() {
            uint id = 0;
            Dispatcher.Invoke(() => {
                pbrBarTot.Maximum += 5550;
            });
            Timers[3] = new WinMMWrapper(1, 0, WinMMWrapper.TimerEventType.Repeating, () => {
                Dispatcher.Invoke(() => {
                    lblCounterTot.Text = TotalCount.ToString();
                    pbrBarTot.Value = TotalCount;
                });
                if (TotalCount % 5550 == 0) {
                    WinMMWrapper.timeKillEvent(id);
                    Semaforo!.Signal();
                }
            });
            id = Timers[3].StartElapsedTimer();
        }
    }
}
