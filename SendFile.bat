@echo off

set USER=vruser
set HOST=172.16.19.167
set REMOTE_PATH=/home/vruser/
set LOCAL_FILE=server\

scp -r "%LOCAL_FILE%" %USER%@%HOST%:%REMOTE_PATH%


