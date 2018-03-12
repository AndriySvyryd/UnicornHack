const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');

function getClientConfig(env) {
    const isDevBuild = !(env && env.prod);
    return {
        stats: { modules: false },
        resolve: {
            extensions: ['*', '.js']
        },
        output: {
            path: path.join(__dirname, 'wwwroot', 'dist'),
            publicPath: 'dist/',
            filename: '[name].js',
            library: '[name]_[hash]'
        },
        module: {
            rules: [
                { test: /\.(woff|woff2|eot|ttf)(\?v=\d+\.\d+\.\d+)?$/, use: 'url-loader?limit=25000' },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' },
                {
                    test: /\.css(\?|$)/,
                    use: ExtractTextPlugin.extract({
                        fallback: 'style-loader',
                        use: isDevBuild ? 'css-loader' : 'css-loader?minimize'
                    })
                },
                {
                    test: /\.scss(\?|$)/,
                    use: ExtractTextPlugin.extract({
                        fallback: 'style-loader',
                        use: isDevBuild
                            ? [{ loader: 'css-loader', options: { importLoaders: 1 } }, 'sass-loader']
                            : [{ loader: 'css-loader', options: { minimize: true, importLoaders: 1 } }, 'sass-loader']
                    })
                }
            ]
        },
        entry: {
            vendor: [
                '@aspnet/signalr', '@aspnet/signalr-protocol-msgpack', 'buffer', 'bootstrap-loader/extractStyles',
                'event-source-polyfill', 'isomorphic-fetch', 'lodash', 'mobx', 'mobx-react', 'react', 'react-dom',
                'react-hotkeys', 'mousetrap', 'url-search-params-polyfill', 'jquery', 'popper.js'
            ]
        },
        plugins: [
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            }),
            new ExtractTextPlugin({ filename: 'vendor.css', allChunks: true }),
            new webpack.ProvidePlugin({
                $: "jquery",
                jQuery: "jquery",
                "window.jQuery": "jquery",
                Popper: ['popper.js', 'default'],
                Alert: "exports-loader?Alert!bootstrap/js/dist/alert",
                Button: "exports-loader?Button!bootstrap/js/dist/button",
                Carousel: "exports-loader?Carousel!bootstrap/js/dist/carousel",
                Collapse: "exports-loader?Collapse!bootstrap/js/dist/collapse",
                Dropdown: "exports-loader?Dropdown!bootstrap/js/dist/dropdown",
                Modal: "exports-loader?Modal!bootstrap/js/dist/modal",
                Popover: "exports-loader?Popover!bootstrap/js/dist/popover",
                Scrollspy: "exports-loader?Scrollspy!bootstrap/js/dist/scrollspy",
                Tab: "exports-loader?Tab!bootstrap/js/dist/tab",
                Tooltip: "exports-loader?Tooltip!bootstrap/js/dist/tooltip",
                Util: "exports-loader?Util!bootstrap/js/dist/util"
            }), // Maps these identifiers for Bootstrap
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"'
            }),
            new webpack.optimize.ModuleConcatenationPlugin()
        ].concat(isDevBuild
            ? []
            : [
                new UglifyJsPlugin({
                    uglifyOptions: {
                        ie8: false,
                        ecma: 6
                    }
                })
            ])
    };
}

module.exports = (env) => { return [getClientConfig(env)]; };