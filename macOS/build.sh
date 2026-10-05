#!/bin/bash

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

APP_NAME="Seed Launcher.app"
PUBLISH_DIR="$ROOT_DIR/Publish/osx-arm64"
APP_DIR="$SCRIPT_DIR/$APP_NAME"

function clean() {
    rm -rf "$PUBLISH_DIR"
    rm -rf "$APP_DIR"
    rm -rf $SCRIPT_DIR/SeedLauncher.icns
}

function build(){
    clean

    dotnet publish \
        "$ROOT_DIR/Launcher/Launcher.csproj" \
        -c Release \
        -r osx-arm64 \
        --self-contained true \
        -p:UseAppHost=true \
        -o "$PUBLISH_DIR"

    echo "Creating application bundle..."

    mkdir -p "$APP_DIR/Contents/MacOS"
    mkdir -p "$APP_DIR/Contents/Resources"

    cp -R "$PUBLISH_DIR/"* "$APP_DIR/Contents/MacOS/"
    cp "$SCRIPT_DIR/Info.plist" "$APP_DIR/Contents/Info.plist"

    chmod +x "$APP_DIR/Contents/MacOS/Launcher"

    iconutil -c icns \
        "$SCRIPT_DIR/SeedLauncher.iconset" \
        -o "$SCRIPT_DIR/SeedLauncher.icns"

    cp "$SCRIPT_DIR/SeedLauncher.icns" \
    "$APP_DIR/Contents/Resources/SeedLauncher.icns"

    echo "Created:"
    echo "$APP_DIR"
}

case "$1" in
    build) build;;
    clean) clean;;
    *) build;;
esac
