'use strict';

// Make sure we're in a Browser-like environment before importing polyfills
// This prevents `fetch()` from being imported in a Node test environment
if (typeof window !== 'undefined') {
    // fetch() polyfill for making API calls.
    require('whatwg-fetch');
}