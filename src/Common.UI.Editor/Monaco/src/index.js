import * as monaco from 'monaco-editor';
import * as monacoVim from 'monaco-vim';
import 'monaco-editor/min/vs/editor/editor.main.css';

const editor = monaco.editor.create(document.getElementById('container'), {
  value: '',
  language: 'javascript',
  theme: 'vs-dark',
});

let vimMode = null;

function toggleVimMode(enable) {
  if (enable) {
    vimMode = monacoVim.initVimMode(editor, document.createElement('div'));
  } else {
    if (vimMode) {
      vimMode.dispose();
      vimMode = null;
    }
  }
}

// リサイズイベントのハンドル
window.addEventListener('resize', () => {
    editor.layout();
});

function setEditorContent(content) {
  editor.setValue(content);
}

function getEditorContent() {
  return editor.getValue();
}

// グローバルスコープに関数を追加して、C#から呼び出せるようにする
window.toggleVimMode = toggleVimMode;
window.setEditorContent = setEditorContent;
window.getEditorContent = getEditorContent;
