import * as monaco from 'monaco-editor';
import * as monacoVim from 'monaco-vim';
import 'monaco-editor/min/vs/editor/editor.main.css';

// カスタムNordテーマの定義
monaco.editor.defineTheme('nord', {
    base: 'vs-dark',
    inherit: true,
    rules: [
        { token: '', foreground: 'D8DEE9', background: '2E3440' },
        { token: 'comment', foreground: '616E88' },
        { token: 'keyword', foreground: '81A1C1' },
        { token: 'number', foreground: 'B48EAD' },
        { token: 'string', foreground: 'A3BE8C' },
        { token: 'type', foreground: '8FBCBB' },
        { token: 'function', foreground: '88C0D0' },
        { token: 'variable', foreground: 'D8DEE9' },
        { token: 'constant', foreground: 'D8DEE9' },
    ],
    colors: {
        'editor.foreground': '#D8DEE9',
        'editor.background': '#2E3440',
        'editorCursor.foreground': '#D8DEE9',
        'editor.lineHighlightBackground': '#3B4252',
        'editorLineNumber.foreground': '#4C566A',
        'editor.selectionBackground': '#434C5E',
        'editor.inactiveSelectionBackground': '#434C5E',
        'editorIndentGuide.background': '#4C566A',
        'editorIndentGuide.activeBackground': '#D8DEE9',
    }
});

window.MonacoEnvironment = {
    getWorkerUrl: function (workerId, label) {
        return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
            self.MonacoEnvironment = {
                baseUrl: 'https://unpkg.com/monaco-editor@0.21.2/min/'
            };
            importScripts('https://unpkg.com/monaco-editor@0.21.2/min/vs/base/worker/workerMain.js');
        `)}`;
    }
};

const editor = monaco.editor.create(document.getElementById('container'), {
  value: '',
  language: 'javascript',
  theme: 'nord',
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
    console.log('Entering getEditorContent');

    if (typeof editor === 'undefined') {
        console.error('Editor is not defined');
        return '';
    }

    try {
        console.log('Before getting value');
        var value = editor.getValue();
        console.log('Editor value:', value);
        return value;
    } catch (error) {
        console.error('Error getting editor value:', error);
        return '';
    }
}

function updateVimMap(before, after, mode) {
    monacoVim.VimMode.Vim.map(before, after, mode)
}
function updateVimMapCommand(key, command, option) {
      monacoVim.VimMode.Vim.mapCommand(key, command, option)
}
function updateVimMapCommand2(key, command, option1, option2) {
      monacoVim.VimMode.Vim.mapCommand(key, command, option1, option2)
}

// グローバルスコープに関数を追加して、C#から呼び出せるようにする
window.toggleVimMode = toggleVimMode;
window.setEditorContent = setEditorContent;
window.getEditorContent = getEditorContent;
window.updateVimMap = updateVimMap;
window.updateVimMapCommand = updateVimMapCommand;
window.updateVimMapCommand2 = updateVimMapCommand2;
