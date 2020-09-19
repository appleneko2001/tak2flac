#!/bin/bash

# =============================================================
# tak2flac install script by github@appleneko2001
# version 0.5 beta
# For allow use script required chmod +x ./install-tak2flac.sh
# =============================================================

ENV_PATH="/home/$USER/.profile"
INSTALL_PATH="/home/$USER/.local"
RELEASE_LINK='https://github.com/appleneko2001/tak2flac/releases/download/1.0_Core/tak2flac_linux-x64.zip'
TMPFILE_PATH="/tmp/tak2flac_release.zip"
TMPBIN_PATH="/tmp/tak2flac_release/bin"

OS_NAME="Unknown"
OS_VERS="00.00"
OS_ARCH=$(uname -m)
PACKAGEMAN=apt-get

# Detect distribution

if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS_NAME=$NAME
    OS_VERS=$VERSION_ID
elif type lsb_release >/dev/null 2>&1; then
    OS_NAME=$(lsb_release -si)
    OS_VERS=$(lsb_release -sr)
elif [ -f /etc/lsb-release ]; then
    OS_NAME=$DISTRIB_ID
    OS_VERS=$DISTRIB_RELEASE
elif [ -f /etc/debian_version ]; then 
    OS_NAME=Debian 
    OS_VERS=$(cat /etc/debian_version)
else 
    OS_NAME=$(uname -s)
    OS_VERS=$(uname -r)
fi 

# Check system requirements

if [ $OS_ARCH != 'x86_64' ];
then
    echo dotNet Core does not support other architecture except x86_64 \(x64\)...
    echo Aborting...
    exit
fi

case ${OS_NAME} in
    Ubuntu)
        if [ $OS_VERS = '16.04' ] || [ $OS_VERS = '18.04' ] || [ $OS_VERS = '19.04' ] || [ $OS_VERS = '19.10' ] || [ $OS_VERS = '20.04' ];
        then
            PACKAGEMAN=apt-get
        else
            echo $OS_NAME $OS_VERS are not supported... Aborting...
            exit
        fi
        ;;
    Debian)
        if [ $OS_VERS = '10' ] || [ $OS_VERS = '9' ];
        then
            PACKAGEMAN=apt-get
        else
            echo $OS_NAME $OS_VERS are not supported... Aborting...
            exit
        fi 
        ;; 
    Fedora)
        if [ $OS_VERS = '32' ] || [ $OS_VERS = '31' ] || [ $OS_VERS = '30' ] || [ $OS_VERS = '29' ];
        then
            PACKAGEMAN=dnf
        else
            echo $OS_NAME $OS_VERS are not supported... Aborting...
            exit
        fi 
        ;;
    'Red Hat Enterprise Linux Server')
        if [ $OS_VERS = '8' ] || [ $OS_VERS = '7' ];
        then
            PACKAGEMAN=dnf
        else
            echo $OS_NAME $OS_VERS are not supported... Aborting...
            exit
        fi 
        ;;
esac

# Detecting elevated privileges

if [ $USER != 'root' ];
then
    echo You are using username \"$USER\" running this script,
    echo That means you want install tak2flac only for this user
    echo Continue? \(yes or no\)
    WaitAnswer=true
    while [ $WaitAnswer = 'true' ]
    do
        read Answer
        if [ $Answer = 'yes' ] || [ $Answer = 'y' ];
        then
            WaitAnswer=false
            echo Install tak2flac for \"$USER\"
        elif [ $Answer = 'no' ] || [ $Answer = 'n' ];
        then
            WaitAnswer=false
            echo Canceled. 
            exit
        fi
    done
else
    echo Install tak2flac for all users
    ENV_PATH="/etc/environment"
    INSTALL_PATH="/usr"
fi

# Start install process

FFMPEG_PATH=""
FFPROBE_PATH=""
while true;
do
    /bin/echo -ne Check FFMPEG is installed...
    if [ -f "$(command -v ffmpeg)" ];
    then
        echo "Found"
        FFMPEG_PATH=$(command -v ffmpeg)
        break
    else
        echo "Not found"
        if [ $USER != 'root' ];
        then
            echo Install cannot continue, due not found required components
            echo and we can\'t install it for you.
            echo Aborting...
            exit
        else
            echo Installing...
            $PACKAGEMAN update
            if ! $PACKAGEMAN install ffmpeg -y
            then
                echo Install failed. Aborting...
            fi
        fi
    fi 
done
echo Check ability TAK feature of current FFMPEG Version...
for item in ffmpeg ffprobe;
do
    if [ -z "$("$item" -v quiet -formats | grep tak)" ]
    then
        echo Current version of FFMPEG are not supported TAK format.
        echo Update it for support this feature.
        echo Aborting...
        exit
    else
        echo $item Passed
        if [ "$item" = "ffprobe" ]
        then
            FFPROBE_PATH=$(command -v ffprobe)
        fi
    fi
done 
/bin/echo -ne Check dotNet \(.Net Core\) runtime...
while true
do
    if [ -f "$(command -v dotnet)" ];
    then
        echo "Found" 
        DETECTED_NETCOREAPP=false
        for item in $(dotnet --list-runtimes);
        do
            if [ "$item" = "Microsoft.NETCore.App" ];
            then
                DETECTED_NETCOREAPP=true
            elif [ "$DETECTED_NETCOREAPP" = "true" ];
            then
                if [[ "$item" =~ '3.1' ]];
                then
                    DETECTED_NETCOREAPP=false
                    break
                else
                    echo 
                    echo dotNet 3.1 or newer are not installed.
                    echo Detected version: $item
                    echo Aborting...
                    exit
                fi
            fi 
        done
        break
    else
        echo "Not found"
        if [ $USER != 'root' ];
        then
            echo Install cannot continue, due not found required components
            echo and we can\'t install it for you.
            echo Aborting...
            exit
        else
            echo Installing...
            $PACKAGEMAN update
            if ! $PACKAGEMAN install dotnet-runtime-3.1 -y
            then
                echo Install failed. Aborting...
            fi
        fi
    fi 
done
# read -ra A <<< "$(cat ~/.profile | grep -hn FFMPEG_BINARY)"; for i in "${A}";do echo $i; done
for item in FFMPEG_BINARY FFPROBE_BINARY;
do
    RESULT=$(cat "$ENV_PATH" | grep -hn $item); 
    while IFS= read -r line;
    do  
        IFS=':'
        read -ra lineNum <<< "$line"; 
        if ! [ -z "$lineNum" ]
        then
            echo Found variable $item in line $lineNum, removing...; 
            sed -i ${lineNum}d $ENV_PATH
        fi
    done <<< "$RESULT"
done
IFS=
echo "FFMPEG_BINARY=$FFMPEG_PATH" >> $ENV_PATH
echo "FFPROBE_BINARY=$FFPROBE_PATH" >> $ENV_PATH

echo "Downloading release file..."
CurlExitCode=$(curl -sSLk $RELEASE_LINK > $TMPFILE_PATH)
if ! $CurlExitCode
then
    echo "Download failed... Aborting..."
    exit $CurlExitCode
fi
mkdir -p "$TMPBIN_PATH"
echo "Extracting file..."
unzip -xoq "$TMPFILE_PATH" -d "$TMPBIN_PATH"
rm $TMPFILE_PATH
mv -f "$TMPBIN_PATH/tak2flac" "$INSTALL_PATH/bin/tak2flac"

echo "Applying file mode..."
chmod 0755 "$INSTALL_PATH/bin/tak2flac" 

# Determine binary are installed successfully or not

if [ -f "$(command -v tak2flac)" ];
then
    echo "Install complete! You can use tak2flac now!"
elif [ -f "$INSTALL_PATH/bin/tak2flac" ];
then
    echo "Install complete! But required restart computer or login again."
elseS
fi

# End of file