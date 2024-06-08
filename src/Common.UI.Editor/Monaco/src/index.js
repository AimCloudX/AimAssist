import * as monaco from 'monaco-editor';
import * as monacoVim from 'monaco-vim';
import 'monaco-editor/min/vs/editor/editor.main.css';
//import './editor.main.nord.css';
import { VimMode } from 'monaco-vim';

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

// Monaco Environmentの設定
self.MonacoEnvironment = {
    getWorkerUrl: function (moduleId, label) {
        if (label === 'json') {
            return './vs/language/json/json.worker.js';
        }
        if (label === 'css') {
            return './vs/language/css/css.worker.js';
        }
        if (label === 'html') {
            return './vs/language/html/html.worker.js';
        }
        if (label === 'typescript' || label === 'javascript') {
            return './vs/language/typescript/ts.worker.js';
        }
        return './vs/editor/editor.worker.js';
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
      VimMode.Vim.mapCommand(';', 'ex', 'normal')
      VimMode.Vim.mapCommand(':', 'motion', 'repeatLastCharacterSearch', { forward: true })
      VimMode.Vim.mapCommand('H', 'motion', 'moveToFirstNonWhiteSpaceCharacter')
      VimMode.Vim.mapCommand('L', 'motion', 'moveToEol')
      VimMode.Vim.mapCommand('gj', 'motion', 'moveByLines', { forward: true, linewise: true })
      VimMode.Vim.mapCommand('gk', 'motion', 'moveByLines', { forward: false, linewise: true })
      VimMode.Vim.mapCommand('j', 'motion', 'moveByDisplayLines', { forward: true })
      VimMode.Vim.mapCommand('k', 'motion', 'moveByDisplayLines', { forward: false })
      VimMode.Vim.map('J', '10j', 'normal')
      VimMode.Vim.map('K', '10k', 'normal')
      VimMode.Vim.map('jj', '<Esc>', 'insert')
      VimMode.Vim.map('<Space>,', '<<', 'visual')
      VimMode.Vim.map('<Space>.', '>>', 'visual')

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

window.updateVimKeyBindings = function (keyBindings) {


    //keyBindings.forEach(binding => {
    //    window.vimMode.vim.editor.addKeybinding({
    //        key: binding.key,
    //        command: binding.command
    //    });
    //});
};
