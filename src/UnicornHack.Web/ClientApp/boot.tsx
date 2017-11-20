import './styles/site.css';
import 'bootstrap-loader';
import 'url-search-params-polyfill';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import * as mobx from 'mobx';
//import DevTools from 'mobx-react-devtools'
import { Game } from './components/Game';

function renderApp() {
    mobx.useStrict(true);

    const paramString = new URL(window.location.toString()).search.slice(1);
    let nextParamIndex = paramString.indexOf('?');
    if (nextParamIndex === -1) {
        nextParamIndex = paramString.length;
    }
    const playerName = new URLSearchParams(paramString.slice(0, nextParamIndex)).get("Name") || '';
    const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href')!;
    ReactDOM.render(
        <div>
            {/*<DevTools />*/}
            <Game playerName={playerName} baseUrl={baseUrl} />
        </div>,
        document.getElementById('react-app')
    );
}

renderApp();
