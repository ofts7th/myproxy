#!/bin/bash
export GOROOT=/app/go
export GOPATH=/projects/myproxy/server
go build -i -o /projects/myproxy/server/bin/app /projects/myproxy/server/src/main.go