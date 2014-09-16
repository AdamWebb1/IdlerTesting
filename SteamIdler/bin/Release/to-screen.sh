#!/bin/sh

# For complicated passwords with exclamation marks, turn off Bash history expand.
set +o histexpand

# Start looping for accounts in the ./accounts/ folder.
for filename in $(pwd)/accounts/*.sh
do
	# Get base name of the script.
	basefilename=$(basename $filename%.*)

	# Run SteamIdler for this account in a seperate screen.
	screen -S ${basefilename%%.*} sh $filename
done