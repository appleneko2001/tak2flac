# tak2flac
依靠 FFMpeg 和 FFProbe 的簡易tak分離器

### 需求
* FFMpeg 和 FFProbe 的二進制檔案 建議使用最新版本
* 指定了 FFMpeg 和 FFProbe 二進制檔案位置的環境變數
* (Windows) .Net framework 4 (Windows 7 或者更新的OS不需要 因爲已經附帶)
* (Linux) 支援.Net framework 4 的Mono運作環境

### 如何使用
```sh
tak2flac -i "tak檔案位置" [-o "輸出目錄"] [-cue "附加cue檔案位置"]
```

### 安裝
#### 繁瑣的安裝方式
* 在 Windows 的方式:
  * 下載 [FFMpeg](https://ffmpeg.org/download.html) 如果你的電腦上沒有
  * 解包 FFMpeg 的內容物到你想要放的地方. 比如 C:\Program Files\FFMpeg
  * 定義名爲 FFMPEG_BINARY 和 FFPROBE_BINARY 的環境變數, 變數值應該指定這兩個二進制檔案的位置
  > 右鍵點擊 "計算機" (又或者 "此台電腦") -> "內容" -> "進階系統設定" 在右側的窗體 -> "進階" -> "環境變數..." -> 新的系統變數
  * 下載tak2flac的Release包 內容物解包到可以是在Windows目錄下 又或者Path指定到的地方 (也可以直接解包到跟FFMpeg一起 然後讓Path環境變數匯入這個目錄)
  * 關閉所有cmd實例用於應用環境變數的變動  
  * 起動cmd, 輸入tak2flac
  * 應該會返回空訊息, 否則需要重新設定
 
* 在 Linux 的方式 (需要用到 sudo 提高權限操作): 
  * 因爲 Linux 系統的特殊性 可以使用包管理器 (比如apt, yum之類的) 安裝FFMpeg, mono 使用下面的指令安裝
  ```sh
  sudo apt update
  sudo apt install ffmpeg
  ``` 
  Mono的安裝方式可以[點擊這裏](https://www.mono-project.com/download/stable/#download-lin)去到官方網站的下載說明
  * 寫入下面的內容到你的 ~/.profile (或者有點相似的)
   ```sh
     FFMPEG_BINARY="FFMpeg二進制檔案位置"
     FFPROBE_BINARY="FFProbe二進制檔案位置"
   ```
  * 下載tak2flac的Release包 內容物解包到哪裏都可以 不過要確保Path環境變數有指定到 比如/usr/bin下面 
  * 賦予0755的chmod給tak2flac.exe 如果有需要的話可以改名字成tak2flac
  * 起動終端機 輸入mono tak2flac (又或者你給它改了名字 那就輸入它的完全名字) 
  * 應該會返回空訊息, 否則需要重新設定

#### EZ的安裝方式
暫時還沒有...

### 它是如何運作的
用於正常運作的話 需要帶有內置cuesheet數據的tak媒體檔案 又或者tak和cue檔案 否則什麼都做不了:(

首先應用程式會先使用FFProbe取得媒體訊息 標籤 如果有內置cuesheet數據的話就用內置的 否則使用附加的cue檔案
然後應用程式會分析cuesheet數據 標籤 然後把這些東西做成FFMpeg的起動參數 最後用FFMpeg處理tak分割工作

### 想要幫助改進這個應用程式
歡迎任何的建議或者臭蟲報告!