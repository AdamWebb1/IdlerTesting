#!/bin/sh
# Enter an infinite loop, this will allow the program to restart if Steam connection is lost.
while :
do
	# Run 'Counter-Strike: Source' and 'Dota 2'.
	mono SteamIdler.exe SteamAccountName SteamAccountPassword 240 570

	# Wait a minute before attempting to reconnect to Steam.
	echo Waiting one minute before attempting reconnection..
	sleep 60
done