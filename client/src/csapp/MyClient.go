package csapp

import (
	"fmt"
	"net"
	"io"
	"flag"
	"log"
	"uuid"
)

var myUUID string
var serverConn net.Conn
var err error
var userName string
var serverIp string
var internalServer string

func ClientWork() {
	_internalServer := flag.String("a", "", "")
	localPort := flag.String("b", "", "")
	_userName := flag.String("c", "", "")
	_serverIp := flag.String("d", "", "")

	flag.Parse()
	userName = *_userName
	serverIp = *_serverIp
	internalServer = *_internalServer

	fmt.Println("欢迎使用山东友大内网代理程序，本地端口：" + *localPort + "，内网服务器：" + internalServer)
	l, err := net.Listen("tcp", ":" + *localPort)
	if (err != nil) {
		fmt.Println("本地监听失败")
		return
	}
	fmt.Println("服务监听中")
	for true {
		client, err := l.Accept()
		if err != nil {
			log.Panic(err)
		} else {
			go handleClient(client)
		}
	}
}

func handleClient(client net.Conn) {
	serverConn, err = net.Dial("tcp", serverIp)
	if (err != nil) {
		fmt.Println("服务器连接错误")
		return
	}

	defer func() {
		fmt.Println("关闭连接")
		serverConn.Close()
		client.Close()
	}()

	var n = 0
	var err error
	var b [1024]byte
	fmt.Fprint(serverConn, "auth")

	n, err = serverConn.Read(b[:])
	if n == 0 || err != nil {
		return
	}

	if (string(b[:n]) == "ok") {
		fmt.Fprint(serverConn, myUUID+","+userName+","+internalServer)
	} else {
		return
	}

	n, err = serverConn.Read(b[:])
	if n == 0 || err != nil {
		log.Println(err)
		return
	}

	if (string(b[:n]) == "ok") {
		fmt.Println("开始转发")
		forwardConnection(serverConn, client)
	} else {
		return
	}
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

func init() {
	_myUid, _ := uuid.NewV4()
	myUUID = _myUid.String()
}