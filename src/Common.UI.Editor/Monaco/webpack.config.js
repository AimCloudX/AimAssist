﻿const path = require('path');
const TerserPlugin = require('terser-webpack-plugin');
const MonacoEditorWebpackPlugin = require("monaco-editor-webpack-plugin")

module.exports = {
  mode: 'production',  // 'development' も可能です
  entry: './src/index.js',  // エントリーポイントを1つに設定
  output: {
    filename: 'bundle.js',  // 単一の出力ファイル名
    path: path.resolve(__dirname, 'dist'),
    clean: true,  // 出力ディレクトリをクリーンアップ
  },
    resolve: {
        extensions: ['.ts', '.js'],
    },
    plugins: [new MonacoEditorWebpackPlugin({
        languages: ['javascript', 'typescript']
    })],
	module: {
		rules: [
			{
				test: /\.css$/,
				use: ['style-loader', 'css-loader']
			},
		]
	},
  optimization: {
    minimize: true,
    minimizer: [new TerserPlugin({
      extractComments: false,
    })],
  },
  performance: {
    maxAssetSize: 400000000,  // 400MBに設定、必要に応じて調整
    maxEntrypointSize: 400000000,  // 400MBに設定、必要に応じて調整
  },
};