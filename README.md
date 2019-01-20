# UnitySceneMetricsSample

### Summary

シーンの読み込みにかかった時間をアプリケーションの実装に依存せずに実現できるかの実験プロジェクト

### Methods

1. SceneManagement.Sceneクラスに隠蔽されているGetLoadingStateInternalメソッドをReflectionで呼び出すことで、呼び出し時点のシーンの状態（NotLoading, Loading, Loaded）を取得する
2. 手順1のSceneの状態を監視する処理を、PlayerLoopを用いてUpdateイベントに仕込む

### Issues

- activeSceneと同様のシーンをLoadSceneした時に正常に動作しない
  - SceneManager.loadedSceenなどのイベントは実行されているため、切り替わり途中にUpdateイベントが呼ばれないらしい
- Android, iOSなどの実機で動作しない
