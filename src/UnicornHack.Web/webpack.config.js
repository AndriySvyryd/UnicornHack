const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const bundleOutputDir = './wwwroot/dist';

function getClientConfig(env) {
    const isDevBuild = !(env && env.prod);
    return {
        stats: { modules: false },
        devtool: isDevBuild ? 'eval-source-map' : 'source-map',
        resolve: { extensions: ['.js', '.jsx', '.ts', '.tsx'] },
        entry: { 'main': './ClientApp/boot.tsx' },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: 'dist/'
        },
        module: {
            rules: [
                { test: /\.tsx?$/, include: /ClientApp/, use: 'awesome-typescript-loader?silent=true' },
                { test: /\.(woff|woff2|eot|ttf)(\?v=\d+\.\d+\.\d+)?$/, use: 'url-loader?limit=25000' },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' },
                {
                    test: /\.css(\?|$)/,
                    use: isDevBuild
                        ? [
                            'style-loader',
                            { loader: 'css-loader', options: { sourceMap: true, importLoaders: 2 } },
                            {
                                loader: 'postcss-loader',
                                options: { sourceMap: true, plugins: () => [require('autoprefixer')()] }
                            },
                            { loader: 'resolve-url-loader', options: { sourceMap: true } }
                        ]
                        : ExtractTextPlugin.extract({
                            fallback: 'style-loader',
                            use: [
                                {
                                    loader: 'css-loader',
                                    options: { sourceMap: true, minimize: true, importLoaders: 2 }
                                },
                                {
                                    loader: 'postcss-loader',
                                    options: { sourceMap: true, plugins: () => [require('autoprefixer')()] }
                                },
                                { loader: 'resolve-url-loader', options: { sourceMap: true } }
                            ]
                        })
                },
                {
                    test: /\.scss(\?|$)/,
                    use: isDevBuild
                        ? [
                            'style-loader',
                            { loader: 'css-loader', options: { sourceMap: true, importLoaders: 3 } },
                            {
                                loader: 'postcss-loader',
                                options: {
                                    sourceMap: true,
                                    parser: 'postcss-scss',
                                    plugins: () => [require('autoprefixer')()]
                                }
                            },
                            { loader: 'resolve-url-loader', options: { sourceMap: true } },
                            { loader: 'sass-loader', options: { sourceMap: true } }
                        ]
                        : ExtractTextPlugin.extract({
                            fallback: 'style-loader',
                            use: [
                                {
                                    loader: 'css-loader',
                                    options: { sourceMap: true, minimize: true, importLoaders: 3 }
                                },
                                {
                                    loader: 'postcss-loader',
                                    options: {
                                        sourceMap: true,
                                        parser: 'postcss-scss',
                                        plugins: () => [require('autoprefixer')()]
                                    }
                                },
                                { loader: 'resolve-url-loader', options: { sourceMap: true } },
                                { loader: 'sass-loader', options: { sourceMap: true } }
                            ]
                        })
                }
            ]
        },
        plugins: [
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require('./wwwroot/dist/vendor-manifest.json')
            }),
            new CheckerPlugin(),
            require('autoprefixer')
        ].concat(isDevBuild
            ? [
                new webpack.SourceMapDevToolPlugin({
                    filename: '[file].map',
                    moduleFilenameTemplate:
                        path.relative(bundleOutputDir,
                            '[resourcePath]') // Point sourcemap entries to the original file locations on disk
                })
            ]
            : [
                new UglifyJsPlugin({
                    uglifyOptions: {
                        ie8: false,
                        ecma: 6
                    }
                }),
                new ExtractTextPlugin('site.css')
            ])
    };
}

module.exports = (env) => { return [getClientConfig(env)]; };