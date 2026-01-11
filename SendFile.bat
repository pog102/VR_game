@echo off

set USER=vruser
set HOST=172.16.19.69
set REMOTE_PATH=/home/vruser/
set LOCAL_FILE=.\server.x86_64

scp "%LOCAL_FILE%" %USER%@%HOST%:%REMOTE_PATH%


