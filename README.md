# WpfThreads
## Introduzione al Problema

Se noi volessimo stampare un numero che continua ad incrementarsi e volessimo vedere il numero che si incrementa dovremmo inserire un **delay**.
Esiste la possibilità di "addormentare" il sistema per un tot di millisecondi.

```cs
Thread.Sleep(1000); // 1 secondo
```

Ma questa istruzione **blocca completamente** tutto il sistema che gestisce l'interfaccia grafica, per questo generalmente non è buona pratica creare degli **event handler** che durano per molto tempo.

Per questo motivo dovremmo lanciardse e un **thread separato** su cui spostare su di esso il pezzo di codice che vogliamo eseguire.

---
## Il Multi-Threading

```cs
private void Button_Click(object sender, RoutedEventArgs e){
	Thread thread1 = new Thread(incrementa);
	thread1.start;
}
```

```cs
private void incrementa(){
	for(int i = 0; x < GIRI; x++){
		lblCounter.Text = x.ToString();
		Thread.Sleep(100);
	}
}
```

### I Primi Problemi
Il thread chiamante non riesce ad accedere a questo oggetto perché quell'oggetto è proprietà del thread principale.
Per consentire a diverse parti del codice di interagire in modo sicuro con il thread principale utilizziamo il ***Dispatcher***.

```cs
private void incrementa(){
	for(int i = 0; x < GIRI; x++){
		Dispatcher.Invoke(() => {
			lblCounter.Text = x.ToString();
		})
		
		Thread.Sleep(100);
	}
}
```

La funzione principale del dispatcher è quella di garantire che tutte le operazioni dell'interfaccia utente (UI) vengano eseguite nel thread dell'interfaccia utente. 
Ciò significa che se una operazione richiede di accedere all'interfaccia utente, come ad esempio l'aggiornamento di un controllo o la modifica di un valore, il dispatcher garantisce che tale operazione venga eseguita in modo sicuro nel thread corretto.

### Concorrenza tra i Processi
Tuttavia ci imbattiamo in un altro problema: se il thread non ha ancora terminato la sua esecuzione e ne avviamo altri questi gireranno tutti in parallelo facendo confusione l'uno con l'altro.
Una soluzione plausibile potrebbe essere la **terminazione forzata** di un processo ogni volta che ne viene avviato uno nuovo tramite l'istruzione:

```cs
thread1.Abort();
```

Tuttavia se vogliamo rendere l'avvio di più thread simultanei una situzione accettabile, dobbiamo prestare attenzione alla **concorrenza** sulle **variabili comuni** su cui i thread agiscono.
Se 2 thread accedono alla stessa variable, allo stesso momento, questa per entrambi i thread avrà lo stesso valore e se entrambi la incrementeranno di +1 il risultato invece di essere x + 2 come ci si aspetta potrebbe risultare invece x + 1. 
Per questo dobbiamo garantire la **mutua esclusione** per l'accesso a quella variabile.

```cs
lock(_locker){ ... }
```

***Lock*** è il costrutto di base che è usato per gestire la **sezione critica**, ed noi lo useremo per garantire la **mutua esclusione** su una variabile *counter* che abbiamo dichiarato globalmente.

```cs
int _counter = 0;
```

### Mettere Insieme i Pezzi
Infine la nostra funzione di ***Incrementa*** prenderebbe una forma del genere:

```cs
private void incrementa1(){
	for (int x = 0; x < GIRI; x++){
		lock (_locker){
			_counter++;
		}
		
		Dispatcher.Invoke(() => {
			lblCounter1.Text = _counter.ToString();
		});
		
		Thread.Sleep(1);
	}
}
```

---
