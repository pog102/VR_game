$game = "build\android\game.apk"
$server = "vruser@172.16.19.167"
$remotePath = "/home/vruser"


scp $game "$server:$remotePath"
# ssh $server "bash ~/deploy_game_to_all.sh $remotePath"
