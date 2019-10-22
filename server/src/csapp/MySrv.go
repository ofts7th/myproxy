package csapp

import (
	"net"
	"log"
	"fmt"
	"io"
	"flag"
	"strings"
	"time"
	"os"
	"io/ioutil"
	"strconv"
)

func SrvWork() {
	p, ok := configMap["port"]
	if (!ok) {
		return
	}
	l, err := net.Listen("tcp", ":"+p)
	if err != nil {
		fmt.Println("listen falied")
		return
	}

	fmt.Println("listening")
	for {
		client, err := l.Accept()
		if err != nil {
			log.Panic(err)
		} else {
			go handleClient(client)
		}
	}
}

var activeSession = make(map[string]int64)

func handleClient(client net.Conn) {
	if client == nil {
		return
	}

	fmt.Println("got client")
	defer func() {
		fmt.Println("close client")
		client.Close()
	}()

	var n = 0
	var err error
	var b [1024]byte

	n, err = client.Read(b[:])
	if n == 0 || err != nil {
		return
	}

	if (string(b[:n]) == "auth") {
		fmt.Fprint(client, "ok")
	} else {
		return
	}

	n, err = client.Read(b[:])
	if n == 0 || err != nil {
		return
	}

	ss := strings.Split(string(b[:n]), ",")
	if (len(ss) < 3) {
		return
	}

	var uuid = ss[0]
	var userName = ss[1]
	var serverIp = ss[2]

	v, sessionOk := activeSession[uuid]
	if (sessionOk) {
		if (time.Now().Unix() < v) {
			sessionOk = false
		}
	}

	if (!sessionOk) {
		sessionOk = checkUser(userName)
		if (sessionOk) {
			activeSession[uuid] = time.Now().Unix() + 3600
		} else {
			return
		}
	}

	var internalConn net.Conn
	internalConn, err = net.Dial("tcp", serverIp)
	if err != nil {
		log.Println(err)
		return
	}

	defer func() {
		fmt.Println("close internal connection")
		internalConn.Close()
	}()

	fmt.Fprint(client, "ok")

	fmt.Println("start forwarding")

	forwardConnection(client, internalConn)
}

func forwardConnection(sconn net.Conn, dconn net.Conn) {
	exitChan := make(chan bool, 1)
	go func(s net.Conn, d net.Conn, e chan bool) {
		io.Copy(d, s)
		e <- true
	}(sconn, dconn, exitChan)

	go func(s net.Conn, d net.Conn, e chan bool) {
		io.Copy(s, d)
		e <- true
	}(sconn, dconn, exitChan)

	<-exitChan
}

var ticker_SheckActiveSession = time.NewTicker(3 * time.Second)

func init() {
	c := flag.String("c", "", "")

	flag.Parse()
	if (*c == "") {
		fmt.Println("no config file specified")
		return
	}

	ReadConfig(c)
	ioutil.WriteFile(configMap["pid"], ([]byte)(strconv.Itoa(os.Getpid())), 0666)
	ConnectToDb()
	go checkActiveSession()
}

func checkActiveSession() {
	for {
		select {
		case <-ticker_SheckActiveSession.C:
			var nowTime = time.Now().Unix()
			for k, v := range activeSession {
				if (v < nowTime) {
					delete(activeSession, k)
				}
			}
		}
	}
}

func checkUser(user string) bool {
	rows, err := dbConn.Query("select name from ProxyUser where name = '" + user + "'")
	defer rows.Close()

	if (err != nil) {
		fmt.Println("db query error")
		return false
	}
	if (rows.Next()) {
		return true
	}
	return false
}
