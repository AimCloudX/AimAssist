import * as monaco from 'monaco-editor';
import * as monacoVim from 'monaco-vim';
import 'monaco-editor/min/vs/editor/editor.main.css';

// Create the editor instance
const editor = monaco.editor.create(document.getElementById('container'), {
  value: '',
  language: 'javascript',
  theme: 'vs-dark',
});

// Enable Vim mode
const statusNode = document.createElement('div');
document.body.appendChild(statusNode);
monacoVim.initVimMode(editor, statusNode);
