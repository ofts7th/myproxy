package csapp

import (
	"os"
	"bufio"
	"io"
	"strings"
)

var configMap = make(map[string]string)

func ReadConfig(file *string) {
	fp, err := os.Open(*file)
	if err != nil {
		return
	}
	defer fp.Close()
	bufReader := bufio.NewReader(fp)

	for {
		lineByte, _, err := bufReader.ReadLine()
		if err != nil {
			if err == io.EOF {
				err = nil
				break
			}
		} else {
			var line = string(lineByte[:])
			var ss = strings.Split(line, "=")
			configMap[ss[0]] = ss[1]
		}
	}
}

func SaveConfig() {

}
