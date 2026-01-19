#!/bin/sh

OUT_FILE="ips.txt"
> "$OUT_FILE"   # clear file

adb devices | tail -n +2 | awk 'NF {print $1}' | while read -r dev; do
    echo "Reading IP from $dev..."

    IP=$(adb -s "$dev" shell ip -f inet addr show wlan0 \
        | awk '/inet / {print $2}' \
        | cut -d/ -f1)

    if [ -n "$IP" ]; then
        echo "$IP" >> "$OUT_FILE"
        echo "  -> $IP"
    else
        echo "  -> no IP found"
    fi
done

echo "Saved IPs to $OUT_FILE"

