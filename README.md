# ImageResize
 
    C#命令行版批量图片大小调整、格式转换、旋转。

### 命令行参数

    -f, --file         Required. 需要处理的文件或目录。
    -r, --recursive    是否遍历子目录，在file为目录时生效，默认为false
    -w, --width        新图片宽，指定高度时0为等比例宽，高宽均不指定时为原图宽
    -h, --height       新图片高，指定宽度时0为等比例高，高宽均不指定时为原图高
    -o, --override     是否覆盖原有文件，默认false，如指定，则无论如何均会删除原图
    -e, --ext          输出图片的格式，默认Jpeg，值为MemoryBmp、Bmp、Emf、Wmf、Gif、Jpeg、Png、Tiff、Exif、Icon 之一
    -q, --quiet        是否静默处理, 不显示处理进度，默认为false
    -t, --filter       过滤需要处理的文件后缀，默认jpg jpeg png bmp gif
    -a, --angle        顺时针旋转角度0-360，默认为0， 不旋转

### 调整图片大小 宽不100像素， 高100像素， 输出格式为jpeg，不回显处理进程
```Bash
ImageResize -f "file-path" -w 100 -h 100 -e jpeg -q
```
### 调整图片大小 宽不100像素， 高自动， 输出格式为jpeg
```Bash
ImageResize -f "file-path" -w 100 -e jpeg
```
### 调整图片大小 宽自动， 高100像素， 输出格式为png, 覆盖原文件
```Bash
ImageResize -f "file-path" -h 100 -e png -o
```
### 转换图片格式， 输出格式为gif, 覆盖原文件
```Bash
ImageResize -f "file-path" -e gif -o
```
### 调整图片大小 宽自动， 高100像素， 输出格式为png, 覆盖原文件， 不遍历子目录
```Bash
ImageResize -f "directory-path" -h 100 -e png -o
```
### 调整图片大小 宽自动， 高100像素， 输出格式为png, 覆盖原文件， 遍历子目录
```Bash
ImageResize -f "directory-path" -h 100 -e png -o -r
```
### 调整图片大小 宽自动， 高100像素， 输出格式为png, 覆盖原文件， 遍历子目录, 只处理GIF和PNG文件， 图片旋转90度
```Bash
ImageResize -f "directory-path" -h 100 -e png -o -r -t gif png -a 90
```
