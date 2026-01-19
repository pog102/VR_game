#!/bin/sh

IPS_FILE="ips.txt"
APK="$HOME/game.apk"

while IFS= read -r ip || [ -n "$ip" ]; do
  echo "Connecting to $ip..."
  adb connect "$ip:5555"
done < "$IPS_FILE"

echo "Installing APK on all connected devices..."
adb install -r "$APK"

