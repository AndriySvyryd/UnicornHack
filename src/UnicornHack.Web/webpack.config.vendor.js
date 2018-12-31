const path = require('path');
const webpack = require('webpack');
const TerserPlugin = require('terser-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const OptimizeCSSAssetsPlugin = require("optimize-css-assets-webpack-plugin");

function getClientConfig(env) {
    const isDevBuild = !(env && env.prod);
    return {
        stats: { modules: false },
        mode: isDevBuild ? 'development' : 'production',
        resolve: {
            extensions: ['*', '.js']
        },
        output: {
            path: path.join(__dirname, 'wwwroot', 'dist'),
            publicPath: 'dist/',
            filename: '[name].js',
            library: '[name]_[hash]',
            globalObject: 'this'
        },
        module: {
            rules: [
                { test: /\.(woff|woff2|eot|ttf)(\?v=\d+\.\d+\.\d+)?$/, use: 'url-loader?limit=25000' },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' },
                {
                    test: /\.css(\?|$)/,
                    use: [
                        MiniCssExtractPlugin.loader,
                        isDevBuild ? 'css-loader' : 'css-loader?minimize'
                    ]
                },
                {
                    test: /\.scss(\?|$)/,
                    use: [
                        MiniCssExtractPlugin.loader,
                        isDevBuild
                            ? { loader: 'css-loader', options: { importLoaders: 1 } }
                            : { loader: 'css-loader', options: { minimize: true, importLoaders: 1 } },
                        'sass-loader'
                    ]
                }
            ]
        },
        entry: {
            vendor: [
                '@aspnet/signalr', '@aspnet/signalr-protocol-msgpack', 'buffer', 'bootstrap-loader',
                'event-source-polyfill', 'isomorphic-fetch', 'lodash', 'mobx', 'mobx-react', 'react', 'react-dom',
                'react-hotkeys', 'mousetrap', 'url-search-params-polyfill', 'jquery', 'popper.js'
            ]
        },
        plugins: [
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            }),
            new MiniCssExtractPlugin({ filename: 'vendor.css' }),
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
            }) // Maps these identifiers for Bootstrap
        ],
        optimization: {
            concatenateModules: true,
            noEmitOnErrors: true,
            splitChunks: {
                cacheGroups: {
                    styles: {
                        name: 'styles',
                        test: /\.css$/,
                        chunks: 'all',
                        enforce: true
                    }
                }
            },
            minimizer: isDevBuild
                ? []
                : [
                    new TerserPlugin({
                        terserOptions: {
                            ecma: 6
                        },
                        cache: true,
                        parallel: true
                    }),
                    new OptimizeCSSAssetsPlugin({})
                ]
        }
    };
}

module.exports = (env) => { return [getClientConfig(env)]; };