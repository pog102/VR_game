$dir = "build\server"
$server = "vruser@172.16.19.167"
$remotePath = "/home/vruser"


scp -r $dir "$server:$remotePath"
