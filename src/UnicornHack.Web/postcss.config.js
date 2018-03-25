module.exports = {
    sourceMap: true,
    parser: 'postcss-scss',
    plugins: [
        require('precss'),
        require('autoprefixer')
    ]
}