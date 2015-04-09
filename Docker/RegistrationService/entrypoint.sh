#!/bin/bash

if [ -z "$KEY" ]; then
	echo >&2 'error: missing KEY environment variable'
	exit 1
fi

openssl aes-256-cbc -d -in /etc/member.config.aes -out /etc/member.config -k $KEY

TEST_LINE=`grep "AuthenticationKey" /etc/member.config`

if [ -z "$TEST_LINE" ]; then
	echo >&2 'error: presented KEY wrong'
	exit 2
fi

/bin/bash -c "$1"

