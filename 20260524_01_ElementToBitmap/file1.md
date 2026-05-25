* 要素からBitmapSourceを作成
* 要素の見た目通りのBitmapSourceを作成
* LayoutTransformによる回転拡大などの変形に対応
* RenderTransformによる変形には非対応
* マウスドラッグ移動でスクロールバーを移動


---

# マウスドラッグ移動でスクロールバーを移動
WPF、ScrollViewerの中の要素をマウスドラッグ移動しているように見せかける - 午後わてんのブログ<br>
https://gogowaten.hatenablog.com/entry/15755956<br>
これの改変、このときはImageのイベントで行っていたけど、今回はScrollViewerのクリックイベントで行うように改良した、このほうが自然だし、ScrollViewer内だけどImageの外側、という微妙な位置でのドラッグ移動もできる<br>



