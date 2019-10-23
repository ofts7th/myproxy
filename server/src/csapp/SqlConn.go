package csapp

import (
	"fmt"
	"database/sql"
	"time"
	_ "mysql"
)

var dbConn *sql.DB

func ConnectToDb() {
	dsn := fmt.Sprintf("%s:%s@tcp(%s:3306)/sdyouda", configMap["dbuser"], configMap["dbpass"], configMap["dbsrv"])
	var err error = nil
	dbConn, err = sql.Open("mysql", dsn)
	if err != nil {
		fmt.Printf("Open mysql failed,err:%v\n", err)
		return
	}
	dbConn.SetConnMaxLifetime(60 * time.Second)
	dbConn.SetMaxOpenConns(5)
	dbConn.SetMaxIdleConns(5)
}