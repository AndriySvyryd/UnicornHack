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
                '@aspnet/signalr-client', 'buffer', 'bootstrap-loader/extractStyles', 'event-source-polyfill', 'isomorphic-fetch',
                'lodash', 'mobx', 'mobx-react', 'react', 'react-dom', 'react-hotkeys', 'mousetrap',
                'url-search-params-polyfill', 'jquery'
            ]
        },
        plugins: [
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            }),
            new ExtractTextPlugin({ filename: 'vendor.css', allChunks: true }),
            new webpack.ProvidePlugin({
                $: 'jquery',
                jQuery: 'jquery'
            }), // Maps these identifiers to the jQuery package (because Bootstrap expects it to be a global variable)
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"'
            })
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