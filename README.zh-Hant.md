[Cannot understand? Click here to show English readme! (but my english could very bad so plz do not be mad)](https://github.com/appleneko2001/tak2flac/blob/master/README.md)

# tak2flac
依靠 FFMpeg 和 FFProbe 的簡易TAK (托馬斯無損音質壓縮編碼) 音訊檔分離器. 關於TAK音訊檔的更多資訊: [維基百科](https://zh.wikipedia.org/wiki/TAK), [Hydrogenaudio 百科](https://wiki.hydrogenaud.io/index.php?title=TAK), [百度百科 (簡體中文)](https://baike.baidu.com/item/Tak)

### 需求
* FFMpeg 和 FFProbe 的二進制檔案 建議使用最新版本
* 指定了 FFMpeg 和 FFProbe 二進制檔案位置的環境變數
* (Windows) .Net framework 4 (Windows 7 或者更新的OS不需要 因爲已經附帶)
* (Linux) ~~支援.Net framework 4 的Mono運作環境~~ 用dotNet Core 代替方案! [官方Linux設定說明](https://docs.microsoft.com/dotnet/core/install/linux-package-managers)
* (Linux) 如果使用dotNet Core, 確定你的Linux OS是64位元的 否則建議用mono

### 如何使用
```sh
tak2flac -i "tak檔案位置" [-o "輸出目錄"] [-cue "附加cue檔案位置"] [-s] [-v] [-b]
    "<input_file>" 選擇需要被切割的媒體檔案 可以是 tak檔案類型 也可以是FFMPEG支援的 並且已經內置了Cuesheet數據
                   如果沒有內置也沒關係. "-i" 爲可選 
    -cue "file.cue" 選擇外部的Cuesheet數據檔案 如果妳的媒體沒有內置Cuesheet數據
    -s  使用新的方式建立時間線 (大部分情況下會比舊的方式縮短了一些)
    -v  顯示執行過程  (Verbose output)
    -b  使用順序執行 (關閉一次性完成工作功能 分序執行)
範例:
    tak2flac "media.tak"  // 最簡單的方式
    tak2flac -i "media.tak" -s  // 選擇media.tak, 並使用新的建立時間線方式.
    tak2flac "media.flac" -cue "media.cue" -s -b  // 選擇media.flac, 並選擇外部cuesheet數據檔案, 關閉"一次性完成"功能避免部分問題
    tak2flac -i "audio_media.flac" -cue "audio_media.cue" -o "./output" -s -b -v  // 選擇audio_media.flac, 並選擇外部cuesheet數據檔案
                // 指定一個輸出目錄 啟用顯示執行過程 啟用新的時間線計算 並且關閉"一次性完成"功能 
```

### 安裝
#### 繁瑣的安裝方式
* 在 Windows 的方式:
  * 下載 [FFMpeg](https://ffmpeg.org/download.html) 如果你的電腦上沒有
  * 解包 FFMpeg 的內容物到你想要放的地方. 比如 C:\Program Files\FFMpeg
  * 定義名爲 FFMPEG_BINARY 和 FFPROBE_BINARY 的環境變數, 變數值應該指定這兩個二進制檔案的位置
  > 右鍵點擊 "計算機" (又或者 "此台電腦") -> "內容" -> "進階系統設定" 在右側的窗體 -> "進階" -> "環境變數..." -> 新的系統變數
  * 下載tak2flac的Release包 內容物解包到可以是在Windows目錄下 又或者Path指定到的地方 (也可以直接解包到跟FFMpeg一起 然後讓Path環境變數匯入這個目錄)
  * 起動cmd, 輸入tak2flac
  * 應該會返回空訊息, 否則需要重新設定
 
* 在 Linux 的方式 (需要用到 sudo 提高權限操作): 
  * 因爲 Linux 系統的特殊性 可以使用包管理器 (比如apt, yum之類的) 安裝FFMpeg, mono 使用下面的指令安裝
  ```sh
  sudo apt update
  sudo apt install ffmpeg
  ``` 
  ~~Mono的安裝方式可以點擊這裏去到官方網站的下載說明~~ 去用dotNet Core版本啦幹 
  * 寫入下面的內容到你的 ~/.profile (或者有點相似的)
   ```sh
     FFMPEG_BINARY="FFMpeg二進制檔案位置"
     FFPROBE_BINARY="FFProbe二進制檔案位置"
   ```
  > 設定完環境變數檔案後可能不會立即變動 需要重新開機 某些情況下
  * 下載tak2flac的Release包 內容物解包到哪裏都可以 不過要確保Path環境變數有指定到 比如/usr/bin下面 (其實也可以放在~/.local/bin目錄下的 如果.profile有自動偵測並設定Path的功能的話也是可以的 這樣也不需要 sudo 不過就只能由這個用戶使用)
  * 賦予0755的chmod給tak2flac
  * 起動終端機 輸入tak2flac
  * 應該會返回空訊息, 否則需要重新設定

#### EZ的安裝方式
暫時還沒有給Windows用的方案 但是我們有給Linux OS用的安裝腳本!
```sh
curl -sS https://raw.githubusercontent.com/appleneko2001/tak2flac/master/install-tak2flac.sh > install-tak2flac.sh
chmod +x ./install-tak2flac.sh
./install-tak2flac.sh
```

### 它是如何運作的
用於正常運作的話 需要帶有內置cuesheet數據的tak媒體檔案 又或者tak和cue檔案 否則什麼都做不了:(

首先應用程式會先使用FFProbe取得媒體訊息 標籤 如果有內置cuesheet數據的話就用內置的 否則使用附加的cue檔案
然後應用程式會分析cuesheet數據 標籤 然後把這些東西做成FFMpeg的起動參數 最後用FFMpeg處理tak分割工作

### 想要幫助改進這個應用程式
歡迎任何的建議或者臭蟲報告!
