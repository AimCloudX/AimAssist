import * as monaco from 'monaco-editor';
import * as monacoVim from 'monaco-vim';
import 'monaco-editor/min/vs/editor/editor.main.css';

// Monaco Editor の設定
const setupMonacoEditor = () => {
    // Nord テーマの定義
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

    // Web Worker の設定
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
};

// エディタのインスタンス作成
let editor;
const createEditor = (initialContent = '', language = 'javascript') => {
    editor = monaco.editor.create(document.getElementById('container'), {
        value: initialContent,
        language: language,
        theme: 'nord',
        fontFamily: "'Consolas', 'Courier New', monospace, 'Codicon'",
        fontSize: 14,
        platform: 'windows'
    });

    // リサイズイベントのハンドル
    window.addEventListener('resize', () => {
        editor.layout();
    });
};

//editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.KeyP, () => {
//    editor.trigger('', 'editor.action.quickCommand', null);
//});

// Vim モードの管理
let vimMode = null;
const toggleVimMode = (enable) => {
    if (enable) {
        vimMode = monacoVim.initVimMode(editor, document.createElement('div'));
    } else {
        if (vimMode) {
            vimMode.dispose();
            vimMode = null;
        }
    }
};

// エディタの内容と言語を設定
const setEditorContent = (content, language) => {
    if (editor) {
        editor.setValue(content);
        if (language) {
            monaco.editor.setModelLanguage(editor.getModel(), language);
        }
    }
};

// エディタの言語のみを設定
const setEditorLanguage = (language) => {
    if (editor && language) {
        monaco.editor.setModelLanguage(editor.getModel(), language);
    }
};


const getEditorContent = () => {
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
};

// Vim マッピングの更新
const updateVimMap = (before, after, mode) => {
    monacoVim.VimMode.Vim.map(before, after, mode);
};

const updateVimMapCommand = (key, command, option) => {
    monacoVim.VimMode.Vim.mapCommand(key, command, option);
};

const updateVimMapCommand2 = (key, command, option1, option2) => {
    monacoVim.VimMode.Vim.mapCommand(key, command, option1, option2);
};

// コマンドパレットを開く関数
function openMonacoCommandPalette() {
    if (editor) {
        editor.trigger('keyboard', 'editor.action.quickCommand', null);
    } else {
        console.error('Monaco Editor instance not found');
    }
}

// 初期化
const init = () => {
    setupMonacoEditor();
    createEditor();
};

// グローバルスコープに関数を追加
window.createEditor = createEditor;
window.setEditorContent = setEditorContent;
window.setEditorLanguage = setEditorLanguage;
window.getEditorContent = getEditorContent;
window.toggleVimMode = toggleVimMode;
window.updateVimMap = updateVimMap;
window.updateVimMapCommand = updateVimMapCommand;
window.updateVimMapCommand2 = updateVimMapCommand2;
window.openMonacoCommandPalette = openMonacoCommandPalette;

// 初期化関数の実行
init();
