Pointsの型をPointCollectionからObservableCollectionPointに変更したGeoLine<br>
伴って、PointsをXAMLに直接記入することができるように、TypeConverterを作成した<br>
[TypeConverter(typeof(MyTypeConverterStringObserveablePoints))]をPointsの属性に加えて使用<br>
こちらのほうがいい<br>
Point追加時、削除時に別の処理を簡単に入れられるのが良い<br>
<br><br><br>


* 右クリックメニューから頂点の編集ができる、切り替え
* 頂点の追加は、先頭、末尾、1番目だけにできる
* 最寄りの頂点の間に入れいるのはまだ
* 追加できる範囲は図形のBounds内だけ
* 
